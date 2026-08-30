using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Component;

public partial class SearchComponent : UserControl
{
    private readonly string[] _searchTerms = [];

    private static readonly IBrush SearchHighlightBrush = new SolidColorBrush(Color.FromArgb(96, 54, 120, 212));

    private static readonly StyledProperty<Surah> SurahProperty =
        AvaloniaProperty.Register<SearchComponent, Surah>(
            nameof(Surah));

    private static readonly StyledProperty<string> VerseCountProperty =
        AvaloniaProperty.Register<SearchComponent, string>(
            nameof(VerseCount));

    private SearchComponent()
    {
        InitializeComponent();
    }

    public SearchComponent(Surah surah, string? searchText = null)
        : this()
    {
        _searchTerms = CreateSearchTerms(searchText);
        Surah = surah;
        VerseCount = $" Verses({surah.Verses.Count})";
    }

    public Surah Surah
    {
        get => GetValue(SurahProperty);
        set => SetValue(SurahProperty, value);
    }

    public string VerseCount
    {
        get => GetValue(VerseCountProperty);
        set => SetValue(VerseCountProperty, value);
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
        if (_searchTerms.Length == 0)
            return;

        if (border.Child is not Grid grid)
            return;

        var textBlocks = grid.Children.OfType<TextBlock>().ToArray();
        if (textBlocks.Length < 3)
            return;

        SetHighlightedText(textBlocks[0], verse.Text);
        SetHighlightedText(textBlocks[1], verse.Translation);
        SetHighlightedText(textBlocks[2], verse.Transliteration);
    }


    private void SetHighlightedText(TextBlock? textBlock, string? text)
    {
        if (textBlock is null)
            return;

        if (string.IsNullOrEmpty(text))
            return;

        var inlines = textBlock.Inlines;
        if (inlines is null)
            return;

        inlines.Clear();
        textBlock.Text = string.Empty;

        foreach (var inline in CreateHighlightedInlines(text))
            inlines.Add(inline);
    }


    private IEnumerable<Inline> CreateHighlightedInlines(string text)
    {
        if (_searchTerms.Length == 0)
        {
            yield return new Run { Text = text };
            yield break;
        }

        var ranges = new List<(int Start, int Length)>();

        foreach (var term in _searchTerms)
        {
            var index = 0;

            while ((index = text.IndexOf(term, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                ranges.Add((index, term.Length));
                index += Math.Max(term.Length, 1);
            }
        }

        if (ranges.Count == 0)
        {
            yield return new Run { Text = text };
            yield break;
        }

        ranges.Sort((left, right) =>
        {
            var comparison = left.Start.CompareTo(right.Start);
            return comparison != 0
                ? comparison
                : right.Length.CompareTo(left.Length);
        });

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

        var position = 0;

        foreach (var range in mergedRanges)
        {
            if (range.Start > position)
            {
                yield return new Run
                {
                    Text = text.Substring(position, range.Start - position)
                };
            }

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
        {
            yield return new Run
            {
                Text = text.Substring(position)
            };
        }
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
        // Play
        // =============================

        var playItem = new MenuItem
        {
            Header = "Play",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Play,
                FontSize = 18
            }
        };

        playItem.Click += (_, _) => { PlayVerseRequested?.Invoke(verse); };


        // =============================
        // Add Items
        // =============================

        menu.Items.Add(copyItem);
        menu.Items.Add(bookmarkItem);
        menu.Items.Add(playItem);

        return menu;
    }


    public event Action<Verse>? PlayVerseRequested;
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
}