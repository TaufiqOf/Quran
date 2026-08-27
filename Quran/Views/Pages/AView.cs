using System.Threading.Tasks;
using Avalonia.Controls;

namespace Quran.Views.Pages;

public abstract class AView : UserControl
{
    public abstract Task Load();
}