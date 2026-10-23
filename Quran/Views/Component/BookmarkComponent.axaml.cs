using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Component;

public partial class BookmarkComponent : UserControl
{
    public sealed class BookmarkVerse
    {
        public required Surah Surah { get; init; }
        public required Verse Verse { get; init; }
        
        public string SurahName => $"Surah: ({Surah.Id}) {Surah.Transliteration}";
    }

    public BookmarkHeader Header { get; private set; } = null!;
    public ObservableCollection<BookmarkVerse> BookmarkVerses { get; } = null!;
    public bool CanDeleteHeader { get; set; } = true;
    public event Action<Surah, Verse>? GoToVerseRequested;
    public event Action<BookmarkHeader>? HeaderDeleted;


    public BookmarkComponent(BookmarkHeader bookmarkHeader)
    {
        InitializeComponent();
        Header = bookmarkHeader;
        HeaderSurahNameTextBox.Text = Header.Name;
        CanDeleteHeader = Header.Id != DataManager.DefaultHeader;
        DeleteHeaderButton.IsVisible = CanDeleteHeader;
        BookmarkVerses = new ObservableCollection<BookmarkVerse>(Header.Bookmarks
            .Select(bookmark =>
            {
                var surah = DataManager.Surahs.FirstOrDefault(s => s.Id == bookmark.SurahId);
                var verse = surah?.Verses.FirstOrDefault(v => v.Id == bookmark.VerseId);
                return surah != null && verse != null
                    ? new BookmarkVerse { Surah = surah, Verse = verse }
                    : null;
            })
            .OfType<BookmarkVerse>()
            .ToList());

        BookmarkItemsControl.ItemsSource = BookmarkVerses;
    }

    public void SetSurahFilter(int surahId)
    {
        BookmarkItemsControl.ItemsSource = surahId < 0
            ? BookmarkVerses
            : BookmarkVerses.Where(item => item.Surah.Id == surahId).ToList();
    }

    private void SaveHeaderNameButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataManager.UpdateBookmarkHeaderName(Header.Id, HeaderSurahNameTextBox.Text ?? string.Empty))
            HeaderSurahNameTextBox.Text = Header.Name;
    }

    private async void CopyHeaderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return;

        var text = new StringBuilder(Header.Name);
        foreach (var bookmarkVerse in BookmarkVerses)
        {
            text.Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append(CopyHelper.FormatText(bookmarkVerse.Verse));
        }

        await CopyHelper.CopyClipboard(topLevel, text.ToString());
        e.Handled = true;
    }

    private async void CopyVerseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: BookmarkVerse bookmarkVerse })
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return;

        await CopyHelper.CopyClipboard(topLevel, CopyHelper.FormatText(bookmarkVerse.Verse));
        e.Handled = true;
    }

    private void DeleteHeaderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!CanDeleteHeader || !DataManager.DeleteBookmarkHeader(Header.Id))
            return;

        HeaderDeleted?.Invoke(Header);
    }

    private void DeleteVerseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: BookmarkVerse bookmarkVerse })
            return;

        var bookmark = new Bookmark
        {
            SurahId = bookmarkVerse.Surah.Id,
            VerseId = bookmarkVerse.Verse.Id
        };

        DataManager.RemoveBookmark(bookmark, Header.Id);
        BookmarkVerses.Remove(bookmarkVerse);
        e.Handled = true;
    }

    private void VerseBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Button)
            return;

        if (sender is Border { DataContext: BookmarkVerse bookmarkVerse })
            GoToVerseRequested?.Invoke(bookmarkVerse.Surah, bookmarkVerse.Verse);
    }
}