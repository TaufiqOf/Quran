using System.Threading.Tasks;
using Avalonia.Controls;

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
}