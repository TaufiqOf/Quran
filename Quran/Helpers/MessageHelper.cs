using Avalonia.Controls;
using Quran.Views;
using TextBoxMessageControl = Quran.Views.Component.MessageControl.TextBoxMessageControl;

namespace Quran.Helpers;

public static class MessageHelper
{
    private static CustomMessageWindow _dialog;
    public static bool IsShowing { get; private set; }
    public static Window? MainWindow { get; set; }
    

    public static void ShowMessage(string title, string message, bool isDialog = true)
    {
        var userControl = new TextBoxMessageControl(message);
        ShowMessage(title, userControl, isDialog);
    }

    public static void ShowMessage(string title, UserControl userControl, bool isDialog = true)
    {
        ShowMessage(title, userControl, 520, 320, isDialog);
    }

    public static void ShowMessage(string title, UserControl userControl, int height, int width, bool isDialog = true)
    {
        if (IsShowing)
            return;

        IsShowing = true;

        var owner = MainWindow;
        _dialog = new CustomMessageWindow(
            title)
        {
            WindowStartupLocation = owner is null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner
        };
        _dialog.SetControl(userControl);
        _dialog.Width = width;
        _dialog.Height = height;
        _dialog.Closed += (_, _) => IsShowing = false;

        if (owner?.IsVisible == true && isDialog)
            _ = _dialog.ShowDialog(owner);
        else
            _dialog.Show();
    }

    public static void Close()
    {
        IsShowing = false;
        _dialog?.Close();
    }
}