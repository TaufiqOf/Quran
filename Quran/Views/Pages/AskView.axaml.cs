using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Quran.Helpers;

namespace Quran.Views.Pages;

public partial class AskView : AView
{
    private CancellationTokenSource? _searchCts;
    private bool _sending = false;
    public ObservableCollection<ChatMessageModel> Messages { get; } = new();

    public AskView()
    {
        InitializeComponent();
        ChatItemsControl.ItemsSource = Messages;
    }

    private void SendTextBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !_sending)
        {
            var message = SendTextBox.Text ?? string.Empty;
            Send(message);
            e.Handled = true;
        }
    }

    private void SendButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var message = SendTextBox.Text ?? string.Empty;
        Send(message);
    }

    private async void Send(string message)
    {
        if (_sending)
            return;
        if (string.IsNullOrWhiteSpace(message))
            return;
        _sending = true;
        if (_searchCts != null)
        {
            await _searchCts.CancelAsync();
            _searchCts.Dispose();
        }

        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        // 1. Add User Message
        Messages.Add(new ChatMessageModel { IsUser = true, Content = message });
        SendTextBox.Text = string.Empty;

        // 2. Add Assistant Message Placeholder
        var aiMessage = new ChatMessageModel { IsUser = false, Content = "Thinking..." };
        Messages.Add(aiMessage);

        ScrollToBottom();

        SendButton.IsEnabled = false;
        ProgressBar.IsIndeterminate = true;
        MessageTextBlock.Text = string.Empty;

        try
        {
            var responseText = await AskAiManager.Ask(message, token);

            if (!token.IsCancellationRequested && !responseText.IsSuccess)
            {
                aiMessage.Content = "No answer could be retrieved from context.";
            }
            else
            {
                aiMessage.Content = responseText.Message;
            }
        }
        catch (OperationCanceledException)
        {
            aiMessage.Content = "Operation canceled.";
        }
        catch (Exception ex)
        {
            MessageTextBlock.Text = "An error occurred while getting response.";
            aiMessage.Content = $"Error: {ex.Message}";
        }
        finally
        {
            ProgressBar.IsIndeterminate = false;
            SendButton.IsEnabled = true;
            _sending = false;
            ScrollToBottom();
        }
    }

    private void ScrollToBottom()
    {
        Dispatcher.UIThread.Post(() => { ChatScrollViewer.ScrollToEnd(); }, DispatcherPriority.Background);
    }

    private async void CopyButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var lastAiMessage = Messages.LastOrDefault(m => !m.IsUser)?.Content;

        if (string.IsNullOrWhiteSpace(lastAiMessage))
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(lastAiMessage);
            MessageTextBlock.Text = "Copied last response!";

            await Task.Delay(2000);
            if (MessageTextBlock.Text == "Copied last response!")
            {
                MessageTextBlock.Text = string.Empty;
            }
        }
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

public class ChatMessageModel : INotifyPropertyChanged
{
    private string _content = string.Empty;

    public bool IsUser { get; set; }

    public string Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}