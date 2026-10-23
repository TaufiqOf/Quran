using System;
using System.Collections.Generic;
using System.Linq;
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

    public BookmarkHeader Header { get; private set; }
    public List<BookmarkVerse> BookmarkVerses { get; private set; }
    public bool CanDeleteHeader => Header?.Id != DataManager.DefaultHeader;
    public event Action<Surah, Verse>? GoToVerseRequested;
    public event Action<BookmarkHeader>? HeaderDeleted;


    public BookmarkComponent(BookmarkHeader bookmarkHeader)
    {
        InitializeComponent();
        Header = bookmarkHeader;
        HeaderSurahNameTextBox.Text = Header.Name;
        DeleteHeaderButton.IsVisible = CanDeleteHeader;
        BookmarkVerses = Header.Bookmarks
            .Select(bookmark =>
            {
                var surah = DataManager.Surahs.FirstOrDefault(s => s.Id == bookmark.SurahId);
                var verse = surah?.Verses.FirstOrDefault(v => v.Id == bookmark.VerseId);
                return surah != null && verse != null
                    ? new BookmarkVerse { Surah = surah, Verse = verse }
                    : null;
            })
            .OfType<BookmarkVerse>()
            .ToList();

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

    private void DeleteHeaderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!CanDeleteHeader || !DataManager.DeleteBookmarkHeader(Header.Id))
            return;

        HeaderDeleted?.Invoke(Header);
    }

    private void VerseBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { DataContext: BookmarkVerse bookmarkVerse })
            GoToVerseRequested?.Invoke(bookmarkVerse.Surah, bookmarkVerse.Verse);
    }
}