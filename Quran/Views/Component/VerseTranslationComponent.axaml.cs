using System;
using Avalonia.Input;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component;

public partial class VerseTranslationComponent : AVerseComponent, IDisposable
{
    public VerseTranslationComponent(Surah surah, Verse verse) : base(surah, verse)
    {
        Verse = verse;
        InitializeComponent();

        TextBlockTranslation.Text = verse.Translation;
        TextBlockBookmark.Text = "\uf02e";
        TextBlockBookmark.IsVisible = IsBookMarked;
        TextBlockVerseNumber.Text = $"Verse {verse.Id}";
    }

    public override void Dispose()
    {
        Verse = null;
    }

    public override void UpdateSelectedState()
    {
        if (IsSelected)
            VerseCard.Classes.Add("selected");
        else
            VerseCard.Classes.Remove("selected");
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
        var point = e.GetCurrentPoint(this);
        if(point.Properties.IsLeftButtonPressed)
        {
            OnVerseSelected(Verse);
        }
    }

    public override void VerseBookMark()
    {
        TextBlockBookmark.Text = "\uf02e";
        TextBlockBookmark.IsVisible = IsBookMarked;
    }

    public override void UpdateUi()
    {
        VerseBookMark();
    }
}