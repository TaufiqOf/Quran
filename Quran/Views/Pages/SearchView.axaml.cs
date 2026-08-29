using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Quran.Helpers;
using Quran.Helpers.Search;
using Quran.Models;
using Quran.Views.Component;

namespace Quran.Views.Pages;

public partial class SearchView : AView
{
    private readonly Timer _timer;
    private List<Surah> _results = new();

    public SearchView()
    {
        InitializeComponent();

        _timer = new Timer();
        _timer.Interval = 500;
        _timer.Stop();
        _timer.Elapsed += TimerOnElapsed;
    }


    public override Task Load(params object?[] parameter)
    {
        return Task.CompletedTask;
    }

    private void LinerScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
    }

    private void SearchTextBoxOnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _timer.Stop();
        _timer.Start();
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        Application.Current?.Dispatcher.Invoke(() =>
        {
            var searchText = SearchTextBox.Text;
            var results = SearchManager.PerformSearch(searchText);
            foreach (var item in SearchItemsControl.Items)
            {
                if (item is SearchComponent searchComponent)
                {
                    DetachSearchComponentEvents(searchComponent);
                }
            }

            _results = results;
            SearchItemsControl.Items.Clear();

            foreach (var surah in results)
            {
                var searchComponent = new SearchComponent(surah);

                searchComponent.GoToVerseRequested += SearchComponentOnGoToVerseRequested;
                searchComponent.CopyTranslationRequested += SearchComponentOnCopyTranslationRequested;
                searchComponent.CopyTransliterationRequested += SearchComponentOnCopyTransliterationRequested;
                searchComponent.CopyVerseRequested += SearchComponentOnCopyVerseRequested;
                searchComponent.CopyAllRequested += SearchComponentOnCopyAllRequested;
                searchComponent.BookmarkVerseRequested += ContextMenuHelper.OnBookmarkVerseRequested;

                SearchItemsControl.Items.Add(searchComponent);
            }

        });
    }

    private void DetachSearchComponentEvents(SearchComponent searchComponent)
    {
        searchComponent.GoToVerseRequested -= SearchComponentOnGoToVerseRequested;
        searchComponent.CopyTranslationRequested -= SearchComponentOnCopyTranslationRequested;
        searchComponent.CopyTransliterationRequested -= SearchComponentOnCopyTransliterationRequested;
        searchComponent.CopyVerseRequested -= SearchComponentOnCopyVerseRequested;
        searchComponent.CopyAllRequested -= SearchComponentOnCopyAllRequested;
        searchComponent.BookmarkVerseRequested -= ContextMenuHelper.OnBookmarkVerseRequested;
    }
    private async void SearchComponentOnCopyTranslationRequested(Verse verse)
    {
        await ContextMenuHelper.CopyTranslationRequested(
            TopLevel.GetTopLevel(this), verse);
    }

    private async void SearchComponentOnCopyTransliterationRequested(Verse verse)
    {
        await ContextMenuHelper.CopyTransliterationRequested(
            TopLevel.GetTopLevel(this), verse);
    }

    private async void SearchComponentOnCopyVerseRequested(Verse verse)
    {
        await ContextMenuHelper.CopyVerseRequested(
            TopLevel.GetTopLevel(this), verse);
    }

    private async void SearchComponentOnCopyAllRequested(Verse verse)
    {
        await ContextMenuHelper.VerseComponentOnCopyAllRequested(
            TopLevel.GetTopLevel(this), verse);
    }
    private void SearchComponentOnGoToVerseRequested(Surah surah, Verse verse)
    {
        RequestGotoPage("Quran", surah, verse.Id);
    }


    private async void CopyButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (!_results.Any())
            return;
        var surahTexts = _results
            .Select(surah =>
            {
                var lines = new List<string>
                {
                    $"({surah.Id}) {surah.Name}",
                    surah.Transliteration,
                    surah.Translation,
                    string.Empty
                };

                foreach (var verse in surah.Verses)
                {
                    lines.Add($"Verse {verse.Id}");
                    lines.Add(verse.Text);
                    lines.Add(verse.Transliteration);
                    lines.Add(verse.Translation);
                    lines.Add(string.Empty);
                }

                return string.Join(
                    Environment.NewLine,
                    lines);
            });

        var text = string.Join(
            Environment.NewLine + Environment.NewLine,
            surahTexts);

        if (string.IsNullOrWhiteSpace(text))
            return;

        var clipboard = TopLevel
            .GetTopLevel(this)?
            .Clipboard;

        if (clipboard is null)
            return;

        await clipboard.SetTextAsync(text);
    }
}