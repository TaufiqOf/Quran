using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;

namespace Quran.Views.Pages;

public partial class BookmarksView : AView
{
    private bool _isChangingFilter;
    private bool _isLoaded;
    private bool _isLoading;
    private Vector _lastScrollOffset;

    public BookmarksView()
    {
        InitializeComponent();
    }

    private List<BookmarkComponent> BookmarkComponents { get; } = new();

    public override Task Load(params object?[] parameter)
    {
        _isLoading = true;

        try
        {
            if (_isLoaded || BookmarkComponents.Count > 0)
            {
                _lastScrollOffset = LinerScrollViewer.Offset;
         
                foreach (var bookmarkComponent in BookmarkComponents)
                {
                    bookmarkComponent.GoToVerseRequested -= GoToVerseRequested;
                    bookmarkComponent.HeaderDeleted -= HeaderDeleted;
                }
                BookmarkComponents.Clear();
                LinerItemsControl.Items.Clear();
                SurahComboBox.Items.Clear();
            }

            _isLoaded = true;

            SurahComboBox.Items.Add(new ComboBoxItem
            {
                Content = "All Surahs",
                Tag = -1
            });

            var bookmarkedSurahs = DataManager.Bookmarks
                .SelectMany(header => header.Bookmarks)
                .Select(bookmark => DataManager.Surahs.FirstOrDefault(s => s.Id == bookmark.SurahId))
                .OfType<Surah>()
                .DistinctBy(surah => surah.Id)
                .OrderBy(surah => surah.Id);

            foreach (var surah in bookmarkedSurahs)
            {
                SurahComboBox.Items.Add(new ComboBoxItem
                {
                    Content = surah.ToString(),
                    Tag = surah.Id
                });
            }

            foreach (var bookmarkHeader in DataManager.Bookmarks)
            {
                var component = new BookmarkComponent(bookmarkHeader);
                component.GoToVerseRequested += GoToVerseRequested;
                component.HeaderDeleted += HeaderDeleted;
                BookmarkComponents.Add(component);
                LinerItemsControl.Items.Add(component);
            }

            SurahComboBox.SelectedIndex = 0;
            LinerScrollViewer.Offset = _lastScrollOffset;
        }
        finally
        {
            _isLoading = false;
        }

        return Task.CompletedTask;
    }


    public override async Task Reload(params object?[] parameter)
    {
        _isLoaded = false;
        await Load(parameter);
    }

    private void GoToVerseRequested(Surah surah, Verse verse)
    {
        DataManager.CurrentSurah = surah;
        DataManager.CurrentVerseId = verse.Id;

        RequestGotoPage(
            "Quran",
            surah,
            verse.Id);
    }

    private async void HeaderDeleted(BookmarkHeader header)
    {
        await Reload();
    }

    private async void AddHeaderButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DataManager.AddBookmarkHeader();
        await Reload();
    }


    private void LinerScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
    }

    private void SurahComboBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || _isChangingFilter)
            return;

        if (SurahComboBox.SelectedItem is not ComboBoxItem { Tag: int surahId })
            return;

        _isChangingFilter = true;
        try
        {
            foreach (var bookmarkComponent in BookmarkComponents)
                bookmarkComponent.SetSurahFilter(surahId);
        }
        finally
        {
            _isChangingFilter = false;
        }

    }
}