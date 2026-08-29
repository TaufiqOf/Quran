using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;

namespace Quran.Views.Pages;

public partial class SearchView : AView
{
    private readonly Timer _timer;
    private List<Surah> _results;

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

    private void SurahComboBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
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
            var mode = GetModeSearchMode(searchText);
            var results = PerformSearch(searchText, mode);
            foreach (var item in SearchItemsControl.Items)
                if (item is SearchComponent searchComponent)
                    searchComponent.GoToVerseRequested -= SearchComponentOnGoToVerseRequested;

            _results = results;
            SearchItemsControl.Items.Clear();
            foreach (var surah in results)
            {
                var searchComponent = new SearchComponent(surah);
                searchComponent.GoToVerseRequested += SearchComponentOnGoToVerseRequested;
                searchComponent.CopyTranslationRequested += VerseComponentOnCopyTranslationRequested;
                searchComponent.CopyTransliterationRequested += VerseComponentOnCopyTransliterationRequested;
                searchComponent.CopyVerseRequested += VerseComponentOnCopyVerseRequested;
                searchComponent.CopyAllRequested += VerseComponentOnCopyAllRequested;
                searchComponent.BookmarkVerseRequested += VerseComponentOnBookmarkVerseRequested;
                SearchItemsControl.Items.Add(searchComponent);
            }
        });
    }


    private void SearchComponentOnGoToVerseRequested(Surah arg1, Verse arg2)
    {
        RequestGotoPage("Quran", arg1, arg2.Id);
    }

    private static List<Surah> PerformSearch(string? searchText, SearchMode mode)
    {
        switch (mode)
        {
            case SearchMode.TextSearch:
                return TextPerformSearch(searchText);
            case SearchMode.StructuredSearch:
                return StructuredPerformSearch(searchText);
            default:
                return new List<Surah>();
        }
    }

    private SearchMode GetModeSearchMode(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return SearchMode.TextSearch;

        var isStructuredSearch = Regex.IsMatch(
            searchText,
            @"^\s*(?:[\p{L}\d]+(?:\s+[\p{L}\d]+)*(?::\d+(?:-\d+)?)?)(?:\s*,\s*(?:[\p{L}\d]+(?:\s+[\p{L}\d]+)*(?::\d+(?:-\d+)? )?))*\s*$",
            RegexOptions.IgnorePatternWhitespace);

        return isStructuredSearch
            ? SearchMode.StructuredSearch
            : SearchMode.TextSearch;
    }

    private static List<Surah> StructuredPerformSearch(string? searchText)
    {
        var list = new List<Surah>();

        if (string.IsNullOrWhiteSpace(searchText))
            return list;

        var searches = searchText.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        var regex = new Regex(
            @"^(?<surah>.+?)(?::(?<startVerse>\d+)(?:-(?<endVerse>\d+))?)?$",
            RegexOptions.IgnoreCase);

        foreach (var search in searches)
        {
            var match = regex.Match(search);

            if (!match.Success)
                continue;

            var surahSearch = match.Groups["surah"]
                .Value
                .Trim();

            int? startVerse = match.Groups["startVerse"].Success
                ? int.Parse(match.Groups["startVerse"].Value)
                : null;

            int? endVerse = match.Groups["endVerse"].Success
                ? int.Parse(match.Groups["endVerse"].Value)
                : null;

            // =====================================
            // Find Surah by ID or Name
            // =====================================

            Surah? surah;

            if (int.TryParse(surahSearch, out var surahId))
                surah = DataManager.Surahs
                    .FirstOrDefault(s => s.Id == surahId);
            else
                surah = DataManager.Surahs
                    .FirstOrDefault(s =>
                        s.Name.Contains(
                            surahSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        s.Transliteration.Contains(
                            surahSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        s.Translation.Contains(
                            surahSearch,
                            StringComparison.OrdinalIgnoreCase));

            if (surah is null)
                continue;

            // =====================================
            // Entire Surah
            // Example: Fatihah
            // =====================================

            if (!startVerse.HasValue)
            {
                list.Add(surah);
                continue;
            }

            // =====================================
            // Normalize verse range
            // =====================================

            var start = startVerse.Value;

            var end = endVerse ?? start;

            var min = Math.Min(start, end);
            var max = Math.Max(start, end);

            // =====================================
            // Find verses
            // =====================================

            var verses = surah.Verses
                .Where(v =>
                    v.Id >= min &&
                    v.Id <= max)
                .ToList();

            if (!verses.Any())
                continue;

            // =====================================
            // Add filtered Surah
            // =====================================

            list.Add(new Surah
            {
                Id = surah.Id,
                Name = surah.Name,
                Translation = surah.Translation,
                Transliteration = surah.Transliteration,
                Verses = verses
            });
        }

        return list;
    }

    private static List<Surah> TextPerformSearch(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return new List<Surah>();


        var words = searchText
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        return DataManager.Surahs
            .Where(s =>
                words.Any(word =>
                    s.Name.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    s.Translation.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    s.Transliteration.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    s.Verses.Any(v =>
                        v.Text.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        v.Translation.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        v.Transliteration.Contains(word, StringComparison.OrdinalIgnoreCase))))
            .Select(s => new Surah
            {
                Id = s.Id,
                Name = s.Name,
                Translation = s.Translation,
                Transliteration = s.Transliteration,

                Verses = s.Verses
                    .Where(v =>
                        words.Any(word =>
                            v.Text.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                            v.Translation.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                            v.Transliteration.Contains(word, StringComparison.OrdinalIgnoreCase)))
                    .ToList()
            })
            .ToList();
    }

    private async void CopyButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
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
    
    private async void VerseComponentOnCopyAllRequested(Verse verse)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard != null)
        {
            var text = $"{verse.Text}\n{verse.Transliteration}\n{verse.Translation}";
            await clipboard.SetTextAsync(text);
        }
    }

    private async void VerseComponentOnCopyTranslationRequested(Verse verse)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard != null) await clipboard.SetTextAsync(verse.Translation);
    }

    private async void VerseComponentOnCopyTransliterationRequested(Verse verse)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard != null) await clipboard.SetTextAsync(verse.Transliteration);
    }

    private async void VerseComponentOnCopyVerseRequested(Verse verse)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard != null) await clipboard.SetTextAsync(verse.Text);
    }
    private void VerseComponentOnBookmarkVerseRequested(Verse verse, Surah surah)
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

    }

}