using System.Threading.Tasks;
using Avalonia.Controls;

namespace Quran.Views.Pages;

public partial class QuranView : AView
{
    public QuranView()
    {
        InitializeComponent();
    }

    public override Task Load()
    {
        return Task.CompletedTask;
    }
}