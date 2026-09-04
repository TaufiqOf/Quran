using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;
using Timer = System.Timers.Timer;

namespace Quran.Views.Pages;

public partial class SearchView : AView
{
    private readonly Timer _messageTimer;
    private readonly Random _random = new();

    private readonly List<string> _searchTips = new()
    {
        "💡 Search Tips",
        "🔍 Search for a word Example: Jesus",
        "📖 Open a specific verse Example: >2:255",
        "📚 Open a range of verses Example: >2:2-5",
        "📖 Open an entire chapter Example: >112:",
        "❓ Ask a question using ? Example: ? Who will go to Heaven?",
        "🔎 Use multiple words to narrow your search Example: Jesus Mary",
        "💡 Try different or simpler keywords if you don't find what you need.",
        "📖 Verse format: Chapter:Verse Example: >2:255",
        "📚 Verse range format: Chapter:Start-End Example: >2:2-5",

        // Semantic Vector Search Options
        "🤖 Perform semantic AI search with ? Example: ? >paradise:10. Here 10 is the number of results to return.",
        "🎯 Limit semantic results count using :N Example: ? >mercy:5, Here 5 is the number of results to return.",
        "🧠 Search concepts, not just words Example: ? reward for good deeds",
        "🌐 Ask semantic questions in any language Example: ? What is the night of decree?",

        // Exact Text / Keyword Search Options
        "🔤 Search exact English translation words Example: Paradise",
        "🔤 Search case-sensitive sky vs. heaven Example: Heaven",
        "🔀 Find verses with all terms Example: Moses Pharaoh",
        "💬 Search by transliteration text Example: >Al-Jannah",
        "🕌 Search Arabic text directly Example: >الجنة",

        // Navigation & Bookmarks
        "🔖 Search by Surah name Example: >Baqarah:",
        "🔢 Quick jump by Surah number Example: >114:"
    };

    private bool _isSearching;

    private List<SurahResult> _results = new();
    private CancellationTokenSource? _searchCts;

    public SearchView()
    {
        InitializeComponent();

        _messageTimer = new Timer();
        _messageTimer.Interval = 10000;
        _messageTimer.Stop();
        _messageTimer.Elapsed += MessageTimerOnElapsed;
        SearchManager.SearcherRegistered += SearcherRegistered;
        if (SearchManager.IsSearcherRegistered)
        {
            ShowMessage();
            _messageTimer.Start();
        }
        else
        {
            MessageTextBlock.Text =
                "Context Search is not initialized yet. You can still search for verses by keywords, or reference (Chapter:Verse). For example, you can search for '2:255' or 'Jesus'.";
        }
    }

    private void MessageTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        ShowMessage();
    }

    private void ShowMessage()
    {
        if (SearchManager.IsSearcherRegistered)
        {
            var tip = _searchTips[_random.Next(_searchTips.Count)];
            Application.Current?.Dispatcher.Invoke(() => { ShowMessage(tip); });
        }
        else
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                ShowMessage(
                    "Context Search is not initialized yet. You can still search for verses by keywords, or reference (Chapter:Verse). For example, you can search for '2:255' or 'Jesus'.");
            });
        }
    }

    private void ShowMessage(string text)
    {
        MessageTextBlock.Classes.Remove("fade-in");
        MessageTextBlock.Text = text;

        // Re-add the class after removing it
        MessageTextBlock.Classes.Add("fade-in");
    }

    private void SearcherRegistered()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            MessageTextBlock.Classes.Add("fade-in");
            MessageTextBlock.Text =
                "Context Search is now initialized. You can search for questions and get context-aware results. using the '?' prefix. For example, you can search for '? Who will go to Heaven?' or '2:255'.";
            _messageTimer.Start();
        });
    }

    public override async Task Load(params object?[] parameter)
    {
    }

    public override async Task Reload(params object?[] parameter)
    {
        await Load(parameter);
    }

    private void LinerScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
    }

    private void SearchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!_isSearching)
        {
            ExecuteSearch();
            SearchButton.Focus();
        }
        else
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = null;
            _isSearching = false;
        }
    }

    private void ExecuteSearch()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;
        _isSearching = true;

        var text = string.Empty;
        Application.Current?.Dispatcher.Invoke(() =>
        {
            foreach (var item in SearchItemsControl.Items)
                if (item is SearchComponent searchComponent)
                    DetachSearchComponentEvents(searchComponent);

            SearchButton.Content = new SymbolIcon
            {
                Symbol = Symbol.Stop,
                FontSize = 16
            };
            ToolTip.SetTip(SearchButton, "Stop Search");
            SearchItemsControl.Items.Clear();
            ProgressBar.IsIndeterminate = true;
            text = SearchTextBox.Text ?? string.Empty;
        });

        if (string.IsNullOrWhiteSpace(text))
        {
            Application.Current?.Dispatcher.Invoke(() => ProgressBar.IsIndeterminate = false);
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                var st = Stopwatch.StartNew();
                var results = await SearchManager.PerformSearch(text, token);

                st.Stop();

                token.ThrowIfCancellationRequested();

                var totalVerses = results.Sum(s => s.VerseResults.Count);
                var surahText = results.Count == 1 ? "surah" : "surahs";
                var verseText = totalVerses == 1 ? "verse" : "verses";
                var durationText =
                    $"Search completed in {st.Elapsed.TotalSeconds:F2} s. Found {totalVerses} {verseText} in {results.Count} {surahText}.";

                var sText = text.Replace("?", string.Empty).Split(':')[0].Trim();
                if (Application.Current != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        _messageTimer.Stop();
                        ShowMessage(durationText);
                        _messageTimer.Start();

                        _results = results;

                        // Populate Controls on UI Thread
                        foreach (var surah in results)
                        {
                            var searchComponent = new SearchComponent(surah, sText);

                            searchComponent.GoToVerseRequested += SearchComponentOnGoToVerseRequested;
                            searchComponent.CopyTranslationRequested += SearchComponentOnCopyTranslationRequested;
                            searchComponent.CopyTransliterationRequested +=
                                SearchComponentOnCopyTransliterationRequested;
                            searchComponent.CopyVerseRequested += SearchComponentOnCopyVerseRequested;
                            searchComponent.CopyAllRequested += SearchComponentOnCopyAllRequested;
                            searchComponent.BookmarkVerseRequested += ContextMenuHelper.OnBookmarkVerseRequested;

                            SearchItemsControl.Items.Add(searchComponent);
                        }
                        ProgressBar.IsIndeterminate = false;
                    }, DispatcherPriority.Normal, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled by a newer search event - safely ignore
            }
            finally
            {
                if (Application.Current != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ProgressBar.IsIndeterminate = false;
                        SearchButton.Content = new SymbolIcon
                        {
                            Symbol = Symbol.Search,
                            FontSize = 16
                        };
                        ToolTip.SetTip(SearchButton, "Search");
                        _isSearching = false;
                        SearchTextBox.Focus();
                    });
                }
            }
        }, token);
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

                foreach (var verse in surah.VerseResults)
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

    private void SearchTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ExecuteSearch();
            e.Handled = true;
        }
    }
}