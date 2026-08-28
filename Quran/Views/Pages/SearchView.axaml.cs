using System.Threading.Tasks;
using Avalonia.Controls;

namespace Quran.Views.Pages;

public partial class SearchView : AView
{
    public SearchView()
    {
        InitializeComponent();
    }

    public override Task Load(params object?[] parameter)
    {
        return Task.CompletedTask;
    }
}