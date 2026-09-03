using Avalonia.Controls;
using Quran.Models;

namespace Quran.Views.Component.MessageControl;

public partial class VerseMessageControl : UserControl
{
    public VerseMessageControl(Surah surah, Verse verse, string message)
    {
        InitializeComponent();
        TextBlockSurahMeta.Text = $"({surah.Id}) {surah.Transliteration} - {surah.Name}";
        TextBlockVerseNumber.Text = $"Verse {verse.Id}";
        TextBoxMessage.Text = message;
    }
}