using System;
using Avalonia.Input;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component;

public partial class VerseComponent : AVerseComponent, IDisposable
{
    private readonly bool _showSurah;

    public VerseComponent(Surah surah, Verse verse, bool showSurah = false) : base(surah, verse)
    {
        _showSurah = showSurah;
        Verse = verse;
        InitializeComponent();

        TextBlockArabic.Text = verse.Text;
        TextBlockTranslation.Text = verse.Translation;
        TextBlockTransliteration.Text = verse.Transliteration;
        TextBlockBookmark.Text = "\uf02e";
        TextBlockBookmark.IsVisible = IsBookMarked;
        if (showSurah)
            TextBlockVerseNumber.Text = $"{surah.Id}. {surah.Transliteration} Verse {verse.Id}";
        else
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
        OnVerseSelected(Verse);
    }

    public override void VerseBookMark()
    {
        TextBlockBookmark.Text = "\uf02e";
        TextBlockBookmark.IsVisible = IsBookMarked;
        if (_showSurah)
            TextBlockVerseNumber.Text = $"{Surah?.Id}. {Surah?.Transliteration} Verse {Verse?.Id}";
        else
            TextBlockVerseNumber.Text = $"Verse {Verse?.Id}";
    }

    public override void UpdateUi()
    {
        VerseBookMark();
    }

    public void DontShowPlay()
    {
        PlayItemManuMenuItem?.IsVisible = false;
    }
}