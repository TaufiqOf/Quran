using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;
using Quran.Views.Component.MessageControl;

namespace Quran.Views.Pages;

public partial class BookmarksView : AView
{
    private VerseMessageControl? _control;
    private bool _isChangingFilter;
    private bool _isLoaded;
    private bool _isLoading;
    private Vector _lastScrollOffset;

    public BookmarksView()
    {
        InitializeComponent();
    }

    private int CurrentSurahId { get; set; } = -1;

    private List<VerseComponent> VerseComponents { get; } = new();

    public override async Task Load(params object?[] parameter)
    {
        var topLevel = TopLevel.GetTopLevel(MessageHelper.MainWindow);
        _isLoading = true;

        try
        {
            if (_isLoaded)
            {
                _lastScrollOffset = LinerScrollViewer.Offset;
                foreach (var verseComponent in VerseComponents)
                {
                    verseComponent.PointerReleased -= VerseComponentPointerReleased;
                    verseComponent.BookmarkVerseRequested -= VerseComponentBookmarkVerseRequested;
                    verseComponent.TafasirRequested -= VerseComponentOnTafasirRequested;
                }

                VerseComponents.Clear();
                LinerItemsControl.Items.Clear();
                SurahComboBox.Items.Clear();
            }

            _isLoaded = true;

            SurahComboBox.Items.Add(new ComboBoxItem
            {
                Content = "All Surahs",
                Tag = -1
            });

            foreach (var bookmark in DataManager.Bookmarks)
            {
                var surah = DataManager.Surahs
                    .FirstOrDefault(q => q.Id == bookmark.SurahId);

                if (surah is null)
                    continue;

                var verse = surah.Verses
                    .FirstOrDefault(q => q.Id == bookmark.VerseId);

                if (verse is null)
                    continue;

                var verseComponent = new VerseComponent(surah, verse, true);

                verseComponent.PointerReleased += VerseComponentPointerReleased;
                verseComponent.BookmarkVerseRequested += VerseComponentBookmarkVerseRequested;
                verseComponent.CopyTranslationRequested += async v =>
                    await ContextMenuHelper.CopyTranslationRequested(topLevel, v);
                verseComponent.CopyTransliterationRequested += async v =>
                    await ContextMenuHelper.CopyTransliterationRequested(topLevel, v);
                verseComponent.CopyVerseRequested += async v =>
                    await ContextMenuHelper.CopyVerseRequested(topLevel, v);
                verseComponent.CopyAllRequested += async (s, v) =>
                    await ContextMenuHelper.VerseComponentOnCopyAllRequested(topLevel, s, v);
                verseComponent.TafasirRequested += VerseComponentOnTafasirRequested;
                verseComponent.DontShowPlay();
                VerseComponents.Add(verseComponent);

                if (!SurahComboBox.Items
                        .OfType<ComboBoxItem>()
                        .Any(q => q.Tag is int id && id == surah.Id))
                    SurahComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = $"({surah.Id}) {surah.Transliteration} - {surah.Name}",
                        Tag = surah.Id
                    });
            }

            // Restore selection using Surah ID
            var selectedItem = SurahComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(q =>
                    q.Tag is int id &&
                    id == CurrentSurahId);

            SurahComboBox.SelectedItem =
                selectedItem ??
                SurahComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault();

            // Apply filtering explicitly
            ApplySurahFilter(CurrentSurahId);

            // Wait until controls have been rendered
            await Dispatcher.UIThread.InvokeAsync(
                () => { },
                DispatcherPriority.Loaded);

            // Restore scroll position
            LinerScrollViewer.Offset = _lastScrollOffset;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void VerseComponentOnTafasirRequested(Surah? surah, Verse verse)
    {
        if(surah == null) return;
        var text = await DataManager.GetTafsirAsync(surah.Id, verse.Id);
        if (_control != null && MessageHelper.IsShowing) MessageHelper.Close();

        _control = new VerseMessageControl(new List<VerseMessageModel> { new(surah, verse, text) });

        MessageHelper.ShowMessage("Tafasir", _control, false);
    }

    public override async Task Reload(params object?[] parameter)
    {
        _isLoaded = false;
        await Load(parameter);
    }

    private void VerseComponentBookmarkVerseRequested(
        Verse verse,
        Surah surah)
    {
        var bookmark = new Bookmark
        {
            SurahId = surah.Id,
            VerseId = verse.Id
        };

        if (DataManager.IsBookmarked(surah.Id, verse.Id))
            DataManager.RemoveBookmark(bookmark);
        else
            DataManager.AddBookmark(bookmark);

        var verseComponent = VerseComponents.FirstOrDefault(q =>
            q.Surah?.Id == surah.Id &&
            q.Verse?.Id == verse.Id);

        if (verseComponent is null)
            return;

        VerseComponents.Remove(verseComponent);
        LinerItemsControl.Items.Remove(verseComponent);
        // Reset scroll after changing the filter
        LinerScrollViewer.Offset = new Vector(0, 0);
        _lastScrollOffset = new Vector(0, 0);
        if (VerseComponents.Count(q => q.Surah?.Id == CurrentSurahId) == 0)
        {
            // If no more verses in the current Surah, reset to All Surahs
            SurahComboBox.Items.Remove(SurahComboBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(q => (int)(q.Tag ?? throw new InvalidOperationException()) == surah.Id));

            CurrentSurahId = -1;
            SurahComboBox.SelectedItem =
                SurahComboBox.Items.OfType<ComboBoxItem>()
                    .FirstOrDefault(q => (int)(q.Tag ?? throw new InvalidOperationException()) == -1);
            ApplySurahFilter(CurrentSurahId);
        }
    }

    private void VerseComponentPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        if (sender is not VerseComponent verseComponent)
            return;

        var surah = verseComponent.Surah;

        DataManager.CurrentSurah = surah;
        DataManager.CurrentVerseId = verseComponent.Verse?.Id;

        RequestGotoPage(
            "Quran",
            surah,
            verseComponent.Verse?.Id);
    }

    private async void SurahComboBoxOnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (_isLoading ||
            _isChangingFilter ||
            SurahComboBox.SelectedItem is not ComboBoxItem selectedItem ||
            selectedItem.Tag is not int surahId)
            return;

        _isChangingFilter = true;

        try
        {
            CurrentSurahId = surahId;

            // Clear and rebuild items
            ApplySurahFilter(surahId);

            // Force layout recalculation
            LinerItemsControl.InvalidateMeasure();
            LinerItemsControl.InvalidateArrange();
            LinerScrollViewer.InvalidateMeasure();
            LinerScrollViewer.InvalidateArrange();

            // Wait for Avalonia layout
            await Dispatcher.UIThread.InvokeAsync(
                () => { },
                DispatcherPriority.Background);

            // Reset after layout has recalculated extent
            LinerScrollViewer.Offset = new Vector(0, 0);

            _lastScrollOffset = new Vector(0, 0);
        }
        finally
        {
            _isChangingFilter = false;
        }
    }

    private void ApplySurahFilter(int surahId)
    {
        LinerItemsControl.Items.Clear();
        foreach (var verseComponent in VerseComponents)
            if (surahId == -1 ||
                verseComponent.Surah?.Id == surahId)
                LinerItemsControl.Items.Add(verseComponent);
    }

    private void LinerScrollViewerOnScrollChanged(
        object? sender,
        ScrollChangedEventArgs e)
    {
        if (!_isLoading && !_isChangingFilter) _lastScrollOffset = LinerScrollViewer.Offset;
    }
}