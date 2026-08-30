using System.Threading.Tasks;
using Avalonia.Interactivity;
using Quran.Helpers;

namespace Quran.Views.Pages;

public partial class SettingsView : AView
{
    public SettingsView()
    {
        InitializeComponent();
    }

    public override Task Load(params object?[] parameter)
    {
        return Task.CompletedTask;
    }


    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        MessageHelper.ShowMessage("Settings Saved", "Your settings have been saved successfully.");
    }
}