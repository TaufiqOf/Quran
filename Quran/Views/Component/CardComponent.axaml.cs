using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component;

public partial class CardComponent : UserControl
{
    private Surah? _surah= null;

    public delegate void CardClickEventHandler(Surah surah);
    public event CardClickEventHandler? CardClick;
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
        _surah = surah;
        TextBlockTitle.Text =$"{surah.Id}. {surah.Transliteration}";
        TextBlockArabicTitle.Text = surah.Name;
        TextBlockSubtitle.Text = surah.Translation ;
        TextBlockMetadata.Text = $"{surah.Type} - {surah.TotalVerses} verses";
        TextBlockTags.Text = string.Join("ꞏ ", synopsis?.Themes ?? []);
        TextBlockDescription.Text = synopsis?.Synopsis;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_surah != null)
        {
            CardClick?.Invoke(_surah);
        }
    }
}