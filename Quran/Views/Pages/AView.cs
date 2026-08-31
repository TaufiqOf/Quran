using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Quran.Views.Pages;

public abstract class AView : UserControl
{
    public delegate void GoToEventHandler(string pageName, params object?[] parameter);

    public abstract Task Load(params object?[] parameter);
    
    public abstract Task Reload(params object?[] parameter);

    public event GoToEventHandler? GotoPageRequested;

    public void RequestGotoPage(string pageName, params object?[] parameter)
    {
        GotoPageRequested?.Invoke(pageName, parameter);
    }
    
    public Action? ReloadRequested { get; set; }
}