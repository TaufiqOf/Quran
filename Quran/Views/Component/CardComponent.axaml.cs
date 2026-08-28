using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component;

public partial class CardComponent : UserControl, IDisposable
{
    public Surah? Surah { get; private set; } = null;

    public delegate void CardClickEventHandler(Surah surah);

    public event CardClickEventHandler? CardClick;

    public bool IsSelected
    {
        get => CardBorder.Classes.Contains("selected");
        set
        {
            if (value)
            {
                CardBorder.Classes.Add("selected");
            }
            else
            {
                CardBorder.Classes.Remove("selected");
            }
        }
    }

    public CardComponent()
    {
        InitializeComponent();
    }

    public CardComponent(Surah surah, SurahSynopsis? synopsis)
    {
        InitializeComponent();
        LoadData(surah, synopsis);
    }

    public void LoadData(Surah surah, SurahSynopsis? synopsis)
    {
        Surah = surah;
        TextBlockTitle.Text = $"{surah.Id}. {surah.Transliteration}";
        TextBlockArabicTitle.Text = surah.Name;
        TextBlockSubtitle.Text = surah.Translation;
        TextBlockMetadata.Text = $"{surah.Type} - {surah.TotalVerses} verses";
        TextBlockTags.Text = string.Join("ꞏ ", synopsis?.Themes ?? []);
        TextBlockDescription.Text = synopsis?.Synopsis;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Surah != null)
        {
            CardClick?.Invoke(Surah);
        }
    }

    public void Dispose()
    {
        Surah = null;
    }
}