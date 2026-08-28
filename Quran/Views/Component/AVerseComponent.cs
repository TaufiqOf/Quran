using System;
using Avalonia;
using Avalonia.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Quran.Models;
using Avalonia.Media;

namespace Quran.Views.Component;

public abstract class AVerseComponent : UserControl, IDisposable
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<AVerseComponent, bool>(
            nameof(IsSelected),
            false);

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Verse? Verse { get; protected set; }

    public delegate void VerseSelectedEventHandler(Verse verse);

    public event VerseSelectedEventHandler? VerseSelected;

    public delegate void VerseContextMenuEventHandler(Verse verse);

    public event VerseContextMenuEventHandler? VerseContextMenuRequested;


    protected AVerseComponent()
    {
        ContextMenu = CreateContextMenu();

        ContextMenu.Opening += ContextMenu_OnOpening;
    }


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
        var copyVerseItem = new MenuItem
        {
            Header = "Copy Verse",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Copy,
                FontSize = 18
            }
        };
        var copyTranslationItem = new MenuItem
        {
            Header = "Copy Translation",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Copy,
                FontSize = 18
            }
        };
        var copyTransliterationItem = new MenuItem
        {
            Header = "Copy Transliteration",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Copy,
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
            Items = { copyVerseItem, copyTranslationItem, copyTransliterationItem }
        };

        var bookmarkItem = new MenuItem
        {
            Header = "Bookmark",
            Icon = new SymbolIcon
            {
                Symbol = Symbol.Bookmark,
                FontSize = 18
            }
        };

        var playItem = new MenuItem
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

        bookmarkItem.Click += (_, _) =>
        {
            if (Verse != null) BookmarkVerseRequested?.Invoke(Verse);
        };

        playItem.Click += (_, _) =>
        {
            if (Verse != null) PlayVerseRequested?.Invoke(Verse);
        };

        menu.Items.Add(copyItem);
        menu.Items.Add(bookmarkItem);
        menu.Items.Add(playItem);

        return menu;
    }


    private void ContextMenu_OnOpening(
        object? sender,
        System.ComponentModel.CancelEventArgs e)
    {
        if (Verse == null)
        {
            e.Cancel = true;
            return;
        }

        IsSelected = true;

        VerseContextMenuRequested?.Invoke(Verse);
    }


    public event Action<Verse>? PlayVerseRequested;

    public event Action<Verse>? BookmarkVerseRequested;

    public event Action<Verse>? CopyVerseRequested;
    public event Action<Verse>? CopyTranslationRequested;
    public event Action<Verse>? CopyTransliterationRequested;

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
}