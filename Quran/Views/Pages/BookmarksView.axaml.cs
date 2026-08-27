using System.Threading.Tasks;
using Avalonia.Controls;

namespace Quran.Views.Pages;

public partial class BookmarksView : AView  
{
    public BookmarksView()
    {
        InitializeComponent();
    }

    public override Task Load(object? parameter)
    {
        return Task.CompletedTask;
    }
}