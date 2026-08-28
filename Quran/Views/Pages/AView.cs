using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Quran.Views.Pages;

public abstract class AView : UserControl
{
    public abstract Task Load(params object?[] parameter);

    public delegate void GoToEventHandler(string pageName, object? parameter = null);

    public event GoToEventHandler? GotoPageRequested;

    public void RequestGotoPage(string pageName, object? parameter = null)
    {
        GotoPageRequested?.Invoke(pageName, parameter);
    }
}