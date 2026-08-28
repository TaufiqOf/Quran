using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Quran.Models;

namespace Quran.Views.Component;

public partial class VerseTranslationComponent : AVerseComponent , IDisposable
{
    public VerseTranslationComponent(Verse verse)
    {
        Verse = verse;
        InitializeComponent();

        TextBlockTranslation.Text = verse.Translation;
        TextBlockVerseNumber.Text = $"Verse {verse.Id}";
    }

    public override void UpdateSelectedState()
    {
        if (IsSelected)
        {
            VerseCard.Classes.Add("selected");
        }
        else
        {
            VerseCard.Classes.Remove("selected");
        }
    }

    private void ButtonBookmark_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
    }

    private void ButtonPlay_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
    }

    private void VerseCard_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        OnVerseSelected(Verse);
    }

    public override void Dispose()
    {
        Verse = null;
    }
}