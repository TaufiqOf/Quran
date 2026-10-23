using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Component;

public abstract class AVerseComponent : UserControl, IDisposable
{
    public delegate void VerseContextMenuEventHandler(Verse verse);

    public delegate void VerseSelectedEventHandler(Verse verse);

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<AVerseComponent, bool>(
            nameof(IsSelected));

    protected MenuItem? PlayItemManuMenuItem;

    private MenuItem? _bookmarkMenuItem;

    protected AVerseComponent(Surah surah, Verse verse)
    {
        Surah = surah;
        Verse = verse;
        ContextMenu = CreateContextMenu();
        ContextMenu.Opening += ContextMenu_OnOpening;
    }
    
    public string BookMarkHeaderId { get; set; } = DataManager.DefaultHeader;

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Surah? Surah { get; protected set; }
    public Verse? Verse { get; protected set; }

    public bool IsBookMarked => DataManager.IsBookmarked(Surah?.Id ?? -1, Verse?.Id ?? -1);

    public virtual void Dispose()
    {
        if (ContextMenu != null)
        {
            ContextMenu.Opening -= ContextMenu_OnOpening;
            ContextMenu = null;
        }

        VerseSelected = null;
        VerseContextMenuRequested = null;
        PlayVerseRequested = null;
        BookmarkVerseRequested = null;
        CopyVerseRequested = null;
        CopyTranslationRequested = null;
        CopyTransliterationRequested = null;

        Verse = null;
    }

    public event Action<Verse>? PlayVerseRequested;
    public event Action<Verse, Surah, string>? BookmarkVerseRequested;
    public event Action<Verse>? CopyVerseRequested;
    public event Action<Verse>? CopyTranslationRequested;
    public event Action<Surah, Verse>? CopyAllRequested;
    public event Action<Verse>? CopyTransliterationRequested;
    public event Action<Surah?, Verse>? TafasirRequested;

    public event VerseSelectedEventHandler? VerseSelected;

    public event VerseContextMenuEventHandler? VerseContextMenuRequested;


    public abstract void UpdateSelectedState();

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty) UpdateSelectedState();
    }


    protected void OnVerseSelected(Verse? verse)
    {
        if (verse == null) return;

        VerseSelected?.Invoke(verse);
    }


    private ContextMenu CreateContextMenu()
    {
        var menu = new ContextMenu
        {
            Padding = new Thickness(4),
            FlowDirection = FlowDirection.LeftToRight
        };
        var copyAllItem = new MenuItem
        {
            Header = "Copy All",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Copy,
                FontSize = 18
            }
        };
        var copyVerseItem = new MenuItem
        {
            Header = "Copy Verse",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.BookOpen,
                FontSize = 18
            }
        };
        var copyTranslationItem = new MenuItem
        {
            Header = "Copy Translation",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Text,
                FontSize = 18
            }
        };
        var copyTransliterationItem = new MenuItem
        {
            Header = "Copy Transliteration",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.CalligraphyPen,
                FontSize = 18
            }
        };

        var copyItem = new MenuItem
        {
            Header = "Copy",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Copy,
                FontSize = 18
            },
            Items = { copyAllItem, copyVerseItem, copyTranslationItem, copyTransliterationItem }
        };

        _bookmarkMenuItem = new MenuItem
        {
            Header = "Bookmark",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Bookmark,
                FontSize = 18
            }
        };

        PlayItemManuMenuItem = new MenuItem
        {
            Header = "Play",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Play,
                FontSize = 18
            }
        };

        copyVerseItem.Click += (_, _) =>
        {
            if (Verse != null) CopyVerseRequested?.Invoke(Verse);
        };
        copyTranslationItem.Click += (_, _) =>
        {
            if (Verse != null) CopyTranslationRequested?.Invoke(Verse);
        };
        copyTransliterationItem.Click += (_, _) =>
        {
            if (Verse != null) CopyTransliterationRequested?.Invoke(Verse);
        };
        copyAllItem.Click += (_, _) =>
        {
            if (Verse != null && Surah != null) CopyAllRequested?.Invoke(Surah, Verse);
        };
        PlayItemManuMenuItem.Click += (_, _) =>
        {
            if (Verse != null) PlayVerseRequested?.Invoke(Verse);
        };


        menu.Items.Add(copyItem);
        menu.Items.Add(_bookmarkMenuItem);
        menu.Items.Add(PlayItemManuMenuItem);

        return menu;
    }


    private void ContextMenu_OnOpening(
        object? sender,
        CancelEventArgs e)
    {
        if (Verse == null)
        {
            e.Cancel = true;
            return;
        }

        UpdateBookmarkMenu();


        VerseContextMenuRequested?.Invoke(Verse);
    }

    private void UpdateBookmarkMenu()
    {
        if (_bookmarkMenuItem == null || Verse == null || Surah == null)
            return;

        _bookmarkMenuItem.Items.Clear();
        _bookmarkMenuItem.IsEnabled = DataManager.Bookmarks.Count > 0;

        foreach (var header in DataManager.Bookmarks)
        {
            var headerId = header.Id;
            var isBookmarked = DataManager.IsBookmarked(Surah.Id, Verse.Id, headerId);
            var headerItem = new MenuItem
            {
                Header = header.Name,
                Icon = new SymbolIcon
                {
                    Symbol = isBookmarked ? Symbol.BookmarkOff : Symbol.Bookmark,
                    FontSize = 18
                }
            };

            headerItem.Click += (_, _) =>
            {
                if (Verse != null && Surah != null)
                    BookmarkVerseRequested?.Invoke(Verse, Surah, headerId);
            };
            _bookmarkMenuItem.Items.Add(headerItem);
        }
    }

    protected void ShowTafasir()
    {
        if (Verse != null) TafasirRequested?.Invoke(Surah, Verse);
    }

    protected void Copy()
    {
        if (Verse != null && Surah != null) CopyAllRequested?.Invoke(Surah, Verse);
    }

    public abstract void VerseBookMark();

    public abstract void UpdateUi();
}