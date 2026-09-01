using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;
using Timer = System.Timers.Timer;

namespace Quran.Views.Pages;

public partial class AskView : AView
{
    public AskView()
    {
        InitializeComponent();
    }

    private void SearchTextBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void SearchButtonOnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void CopyButtonOnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    public override Task Load(params object?[] parameter)
    {
        return Task.CompletedTask;
    }

    public override Task Reload(params object?[] parameter)
    {
        return Task.CompletedTask;
    }
}