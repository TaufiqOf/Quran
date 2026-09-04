using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Component;

public partial class SearchComponent : UserControl
{
    private static readonly IBrush SearchHighlightBrush = new SolidColorBrush(Color.FromArgb(96, 54, 120, 212));

    private static readonly StyledProperty<SurahResult> SurahProperty =
        AvaloniaProperty.Register<SearchComponent, SurahResult>(
            nameof(Surah));

    private static readonly StyledProperty<string> VerseCountProperty =
        AvaloniaProperty.Register<SearchComponent, string>(
            nameof(VerseCount));

    private static readonly StyledProperty<string> FormattedScoreProperty =
        AvaloniaProperty.Register<SearchComponent, string>(nameof(FormattedScore));

    private static readonly StyledProperty<bool> HasScoreProperty =
        AvaloniaProperty.Register<SearchComponent, bool>(nameof(HasScore));

    private readonly string[] _searchTerms = [];

    private SearchComponent()
    {
        InitializeComponent();
    }

    public SearchComponent(SurahResult surah, string? searchText = null)
        : this()
    {
        _searchTerms = CreateSearchTerms(searchText);
        Surah = surah;
        HasScore = surah.SimilarityScore.HasValue;

        // Scale score to 0-100 and format strictly to 2 decimal places
        FormattedScore = surah.SimilarityScore.HasValue
            ? $"Score: {surah.SimilarityScore:P2}"
            : string.Empty;

        VerseCount = $" Verses({surah.VerseResults.Count})";
    }

    public SurahResult Surah
    {
        get => GetValue(SurahProperty);
        set => SetValue(SurahProperty, value);
    }

    public string VerseCount
    {
        get => GetValue(VerseCountProperty);
        set => SetValue(VerseCountProperty, value);
    }

    public string FormattedScore
    {
        get => GetValue(FormattedScoreProperty);
        set => SetValue(FormattedScoreProperty, value);
    }

    public bool HasScore
    {
        get => GetValue(HasScoreProperty);
        set => SetValue(HasScoreProperty, value);
    }

    public event Action<Surah, Verse>? GoToVerseRequested;


    private static string[] CreateSearchTerms(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return [];

        return searchText
            .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(term => term.Trim())
            .Where(term => term.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void ApplyVerseHighlighting(Border border, Verse verse)
    {
        if (border.Child is not Grid grid)
            return;

        var textBlocks = grid.Children.OfType<TextBlock>().ToArray();
        if (textBlocks.Length < 3)
            return;

        // 1. Extract impact words from semantic search (if present)
        var impacts = (verse as VerseResult)?.Impacts;
        var impactTerms = Array.Empty<string>();

        if (impacts != null && impacts.Count > 0)
            impactTerms = impacts
                .Where(i => !string.IsNullOrWhiteSpace(i.VerseWord))
                .Select(i => i.VerseWord.Trim('.', ',', ';', ':', '?', '!', '"', '[', ']', '(', ')', '{', '}'))
                .Where(t => t.Length > 0)
                .ToArray();

        // 2. Combine impact terms with explicit raw search terms to guarantee exact matches are never missed
        var combinedTerms = impactTerms
            .Concat(_searchTerms)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (combinedTerms.Length == 0)
            return;

        // 3. Apply highlights to Arabic (0), Translation (1), and Transliteration (2)
        SetHighlightedText(textBlocks[0], verse.Text, combinedTerms);
        SetHighlightedText(textBlocks[1], verse.Translation, combinedTerms);
        SetHighlightedText(textBlocks[2], verse.Transliteration, combinedTerms);
    }

    private void SetHighlightedText(TextBlock? textBlock, string? text, string[] terms)
    {
        if (textBlock is null || string.IsNullOrEmpty(text))
            return;

        var inlines = textBlock.Inlines;
        if (inlines is null)
            return;

        inlines.Clear();
        textBlock.Text = string.Empty;

        foreach (var inline in CreateHighlightedInlines(text, terms))
            inlines.Add(inline);
    }

    private IEnumerable<Inline> CreateHighlightedInlines(string text, string[] terms)
    {
        if (terms.Length == 0)
        {
            yield return new Run { Text = text };
            yield break;
        }

        var ranges = new List<(int Start, int Length)>();

        foreach (var term in terms)
        {
            var index = 0;

            while ((index = text.IndexOf(term, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                // Match whole word boundaries so shorter tokens (e.g. "it") don't partially match inside words ("spirit")
                var startBoundary = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
                var endBoundary = index + term.Length >= text.Length ||
                                  !char.IsLetterOrDigit(text[index + term.Length]);

                if (startBoundary && endBoundary) ranges.Add((index, term.Length));

                index += Math.Max(term.Length, 1);
            }
        }

        if (ranges.Count == 0)
        {
            yield return new Run { Text = text };
            yield break;
        }

        // Sort ranges by start position, then by length descending
        ranges.Sort((left, right) =>
        {
            var comparison = left.Start.CompareTo(right.Start);
            return comparison != 0
                ? comparison
                : right.Length.CompareTo(left.Length);
        });

        // Merge overlapping highlight intervals
        var mergedRanges = new List<(int Start, int Length)>();

        foreach (var range in ranges)
        {
            if (mergedRanges.Count == 0)
            {
                mergedRanges.Add(range);
                continue;
            }

            var lastIndex = mergedRanges.Count - 1;
            var lastRange = mergedRanges[lastIndex];
            var lastEnd = lastRange.Start + lastRange.Length;
            var currentEnd = range.Start + range.Length;

            if (range.Start > lastEnd)
            {
                mergedRanges.Add(range);
                continue;
            }

            mergedRanges[lastIndex] = (
                lastRange.Start,
                Math.Max(lastEnd, currentEnd) - lastRange.Start);
        }

        // Generate output inlines
        var position = 0;

        foreach (var range in mergedRanges)
        {
            if (range.Start > position)
                yield return new Run
                {
                    Text = text.Substring(position, range.Start - position)
                };

            var highlight = new Span
            {
                Background = SearchHighlightBrush,
                FontWeight = FontWeight.SemiBold
            };

            highlight.Inlines.Add(new Run
            {
                Text = text.Substring(range.Start, range.Length)
            });

            yield return highlight;
            position = range.Start + range.Length;
        }

        if (position < text.Length)
            yield return new Run
            {
                Text = text.Substring(position)
            };
    }


    private ContextMenu CreateContextMenu(Verse verse)
    {
        var menu = new ContextMenu
        {
            Padding = new Thickness(4),
            FlowDirection = FlowDirection.LeftToRight
        };

        // =============================
        // Copy All
        // =============================

        var copyAllItem = new MenuItem
        {
            Header = "Copy All",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Copy,
                FontSize = 18
            }
        };

        copyAllItem.Click += (_, _) => { CopyAllRequested?.Invoke(verse); };


        // =============================
        // Copy Verse
        // =============================

        var copyVerseItem = new MenuItem
        {
            Header = "Copy Verse",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.BookOpen,
                FontSize = 18
            }
        };

        copyVerseItem.Click += (_, _) => { CopyVerseRequested?.Invoke(verse); };


        // =============================
        // Copy Translation
        // =============================

        var copyTranslationItem = new MenuItem
        {
            Header = "Copy Translation",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Text,
                FontSize = 18
            }
        };

        copyTranslationItem.Click += (_, _) => { CopyTranslationRequested?.Invoke(verse); };


        // =============================
        // Copy Transliteration
        // =============================

        var copyTransliterationItem = new MenuItem
        {
            Header = "Copy Transliteration",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.CalligraphyPen,
                FontSize = 18
            }
        };

        copyTransliterationItem.Click += (_, _) => { CopyTransliterationRequested?.Invoke(verse); };


        // =============================
        // Copy Submenu
        // =============================

        var copyItem = new MenuItem
        {
            Header = "Copy",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Copy,
                FontSize = 18
            }
        };

        copyItem.Items.Add(copyAllItem);
        copyItem.Items.Add(copyVerseItem);
        copyItem.Items.Add(copyTranslationItem);
        copyItem.Items.Add(copyTransliterationItem);


        // =============================
        // Bookmark
        // =============================

        var isBookmarked =
            DataManager.IsBookmarked(
                Surah.Id,
                verse.Id);

        var bookmarkItem = new MenuItem
        {
            Header = isBookmarked
                ? "Remove Bookmark"
                : "Bookmark",

            Icon = new SymbolIcon
            {
                Symbol = isBookmarked
                    ? Symbol.BookmarkOff
                    : Symbol.Bookmark,

                FontSize = 18
            }
        };

        bookmarkItem.Click += (_, _) =>
        {
            BookmarkVerseRequested?.Invoke(
                verse,
                Surah);
        };

        // =============================
        // Add Items
        // =============================

        menu.Items.Add(copyItem);
        menu.Items.Add(bookmarkItem);

        return menu;
    }


    public event Action<Verse, Surah>? BookmarkVerseRequested;
    public event Action<Verse>? CopyVerseRequested;
    public event Action<Verse>? CopyTranslationRequested;
    public event Action<Verse>? CopyAllRequested;
    public event Action<Verse>? CopyTransliterationRequested;

    private void InputElement_OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(sender as Visual)
            .Properties
            .IsLeftButtonPressed)
            LeftButtonPressed(sender);
        if (e.GetCurrentPoint(sender as Visual)
            .Properties
            .IsRightButtonPressed)
            RightButtonPressed(sender);
        e.Handled = true;
    }

    private void RightButtonPressed(object? sender)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not Verse verse)
            return;

        border.ContextMenu = CreateContextMenu(verse);
    }

    private void LeftButtonPressed(object? sender)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not Verse verse)
            return;

        GoToVerseRequested?.Invoke(Surah, verse);
    }


    private void VerseBorder_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not Verse verse)
            return;

        ApplyVerseHighlighting(border, verse);
    }

    private async void CopyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

        if (clipboard != null)
            foreach (var verse in Surah.VerseResults)
            {
                var text =
                    $"{Surah.Id}-{Surah.Transliteration}\n({verse.Id}){verse.Text}\n{verse.Transliteration}\n{verse.Translation}";
                await clipboard.SetTextAsync(text);
            }
    }
}