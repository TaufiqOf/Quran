using Avalonia.Controls;

namespace Quran.Views.Component.MessageControl;

public partial class TextBoxMessageControl : UserControl
{
    public TextBoxMessageControl(string message)
    {
        InitializeComponent();
        TextBoxMessage.Text = message;
    }
}