using Avalonia.Controls;
using Quran.Views;

namespace Quran.Helpers;

public static class MessageHelper
{
    private static bool _isShowing = false;
    public static Window? MainWindow { get; set; }

    public static void ShowMessage(string title, string message)
    {
        if (_isShowing)
            return;

        _isShowing = true;

        var owner = MainWindow;
        var dialog = new CustomMessageWindow(
            title,
            message)
        {
            WindowStartupLocation = owner is null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner
        };

        dialog.Closed += (_, _) => _isShowing = false;

        if (owner?.IsVisible == true)
            _ = dialog.ShowDialog(owner);
        else
            dialog.Show();
    }
}