using System.Linq;
using Avalonia.Controls;
using Quran.Models;

namespace Quran.Views.Component;

public partial class CardComponent : UserControl
{
    public CardComponent(Surah surah, SurahSynopsis? synopsis)
    {
        InitializeComponent();
        TextBlockTitle.Text =$"{surah.Id}. {surah.Transliteration}";
        TextBlockArabicTitle.Text = surah.Name;
        TextBlockSubtitle.Text = surah.Translation ;
        TextBlockMetadata.Text = $"{surah.Type} - {surah.TotalVerses} verses";
        TextBlockTags.Text = string.Join("ꞏ ", synopsis?.Themes ?? []);
        TextBlockDescription.Text = synopsis?.Synopsis;
    }
    
    

}