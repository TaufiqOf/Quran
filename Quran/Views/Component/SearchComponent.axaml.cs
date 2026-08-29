using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Component;

public partial class SearchComponent : UserControl
{
    private MenuItem _copyVerseMenuItem;

    public static readonly StyledProperty<Surah?> SurahProperty =
        AvaloniaProperty.Register<SearchComponent, Surah?>(
            nameof(Surah));

    public SearchComponent()
    {
        InitializeComponent();
    }

    public SearchComponent(Surah surah)
        : this()
    {
        Surah = surah;
    }

    public Surah? Surah
    {
        get => GetValue(SurahProperty);
        set => SetValue(SurahProperty, value);
    }

    public event Action<Surah, Verse>? GoToVerseRequested;


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
            Surah != null &&
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
            if (Surah is null)
                return;

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
        if (Surah is null)
            return;

        if (sender is not Border border)
            return;

        if (border.DataContext is not Verse verse)
            return;

        GoToVerseRequested?.Invoke(Surah, verse);
    }

  
}