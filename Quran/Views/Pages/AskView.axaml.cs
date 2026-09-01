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
    private CancellationTokenSource? _searchCts;

    public AskView()
    {
        InitializeComponent();
    }

    private void SendTextBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var message = SendTextBox.Text ?? "";
            Send(message);
            e.Handled = true;
        }
    }

    private void SendButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var message = SendTextBox.Text ?? "";
        Send(message);
    }

    private async void Send(string message)
    {
        if(_searchCts != null)
        {
            await _searchCts.CancelAsync();
            _searchCts.Dispose();
        }
        _searchCts = new CancellationTokenSource();
        SendButton.IsEnabled = false;
        SendTextBox.Text = "";
        var token = _searchCts.Token;
        await AskAiManager.Ask(message, token);
        SendButton.IsEnabled = true;
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