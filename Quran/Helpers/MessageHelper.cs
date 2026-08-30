using Avalonia.Controls;
using Quran.Views;
using TextBoxMessageControl = Quran.Views.Component.MessageControl.TextBoxMessageControl;

namespace Quran.Helpers;

public static class MessageHelper
{
    private static bool _isShowing;
    public static Window? MainWindow { get; set; }

    public static void ShowMessage(string title, string message)
    {
        var userControl = new TextBoxMessageControl(message);
        ShowMessage(title, userControl);
    }

    public static void ShowMessage(string title, UserControl userControl)
    {
        if (_isShowing)
            return;

        _isShowing = true;

        var owner = MainWindow;
        var dialog = new CustomMessageWindow(
            title)
        {
            WindowStartupLocation = owner is null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner
        };
        dialog.SetControl(userControl);
        dialog.Closed += (_, _) => _isShowing = false;

        if (owner?.IsVisible == true)
            _ = dialog.ShowDialog(owner);
        else
            dialog.Show();
    }
}