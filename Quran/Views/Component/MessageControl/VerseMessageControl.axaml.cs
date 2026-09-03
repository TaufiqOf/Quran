using Avalonia.Controls;
using Quran.Models;

namespace Quran.Views.Component.MessageControl;

public partial class VerseMessageControl : UserControl
{
    public VerseMessageControl(Surah surah, Verse verse, string message) : base()   
    {
        InitializeComponent();
        TextBoxMessage.Text = message;
    }
}