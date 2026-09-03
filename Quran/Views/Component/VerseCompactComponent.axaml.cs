using System;
using Avalonia.Input;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component;

public partial class VerseCompactComponent : AVerseComponent, IDisposable
{
    public VerseCompactComponent(Surah surah, Verse verse) : base(surah, verse)
    {
        InitializeComponent();

        TextBlockArabic.Text = verse.Text;
        TextBlockTranslation.Text = verse.Translation;
        TextBlockTransliteration.Text = verse.Transliteration;
        TextBlockBookmark.Text = "\uf02e";
        TextBlockBookmark.IsVisible = IsBookMarked;
        TextBlockVerseNumber.Text = $"Verse {Verse?.Id}";
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

        if (point.Properties.IsLeftButtonPressed)
        {
            OnVerseSelected(Verse);
        }
    }

    public override void VerseBookMark()
    {
        TextBlockBookmark.Text = "\uf02e";
        TextBlockBookmark.IsVisible = IsBookMarked;
        TextBlockVerseNumber.Text = $"Verse {Verse?.Id}";
    }

    public override void UpdateUi()
    {
        VerseBookMark();
    }


}