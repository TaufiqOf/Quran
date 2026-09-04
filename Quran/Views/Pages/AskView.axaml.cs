using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component.MessageControl;

namespace Quran.Views.Pages;

public partial class AskView : AView
{
    private VerseMessageControl? _control;
    private CancellationTokenSource? _searchCts;
    private bool _sending;
    private bool _isLoaded;

    public AskView()
    {
        InitializeComponent();
    }

    private ObservableCollection<ChatMessageModel> Messages { get; set; } =
        new ObservableCollection<ChatMessageModel>();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ChatScrollViewer.ScrollToEnd();
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
        Messages.Add(new ChatMessageModel { IsUser = true, Content = message, Time = DateTime.Now });
        SendTextBox.Text = string.Empty;
        // 2. Add Assistant Message Placeholder
        var aiMessage = new ChatMessageModel
        {
            IsUser = false,
            Content = "Getting sources related to the question...",
            Time = DateTime.Now,
            IsWorking = true
        };
        Messages.Add(aiMessage);
        ScrollToBottom();

        SendButton.IsEnabled = false;
        ProgressBar.IsIndeterminate = true;
        MessageTextBlock.Text = string.Empty;
        SettingService.SaveChatMessages(Messages.ToList());
        try
        {
            var messageModel = new AskAiManager.MessageResult
            {
                IsSuccess = false
            };
            messageModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AskAiManager.MessageResult.Context))
                    Dispatcher.UIThread.Post(() =>
                    {
                        aiMessage.Reference = messageModel.Context;
                        aiMessage.Content = "Sources retrieved. Generating answer...";
                    }, DispatcherPriority.Background);
            };
            messageModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AskAiManager.MessageResult.Message))
                    Dispatcher.UIThread.Post(() => { aiMessage.Content = messageModel.Message; },
                        DispatcherPriority.Background);
            };
            await foreach (var chunk in AskAiManager.AskStreaming(message, messageModel, token))
            {
            }

            if (!token.IsCancellationRequested && !messageModel.IsSuccess)
                aiMessage.Content = "No answer could be retrieved from context.";
            else if (messageModel.IsSuccess) aiMessage.Content = messageModel.Message;

            aiMessage.Reference = messageModel.Context;
        }
        catch (OperationCanceledException exception)
        {
            aiMessage.Content = $"Operation canceled. {exception.Message}";
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
            aiMessage.IsWorking = false;
            SettingService.SaveChatMessages(Messages.ToList());
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
            if (MessageTextBlock.Text == "Copied last response!") MessageTextBlock.Text = string.Empty;
        }
    }

    public override Task Load(params object?[] parameter)
    {
        if (!_isLoaded)
        {
            _isLoaded = true;
            Task.Factory.StartNew(() =>
            {
                var savedMessages = SettingService.LoadChatMessages()
                    .OrderByDescending(m => m.Time)
                    .Take(30)
                    .OrderBy(m => m.Time)
                    .ToList();
                savedMessages.ForEach(m => m.IsWorking = false);
                Messages = new ObservableCollection<ChatMessageModel>(savedMessages);
                Dispatcher.UIThread.Post(() => { ChatItemsControl.ItemsSource = Messages; },
                    DispatcherPriority.Background);
            });
        }

        ChatScrollViewer.ScrollToEnd();
        return Task.CompletedTask;
    }

    public override Task Reload(params object?[] parameter)
    {
        return Task.CompletedTask;
    }

    private async void CopyMessageButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ChatMessageModel messageModel)
            await CopyToClipboardAsync(messageModel.Content);
    }

    private async Task CopyToClipboardAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
            MessageTextBlock.Text = "Copied message to clipboard!";

            await Task.Delay(2000);
            if (MessageTextBlock.Text == "Copied message to clipboard!") MessageTextBlock.Text = string.Empty;
        }
    }

    private async void ShowReferenceButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ChatMessageModel messageModel }
            && messageModel.Reference.Any())
            ShowReference(messageModel);
    }


    private async void DeleteMessageButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ChatMessageModel messageModel)
        {
            Messages.Remove(messageModel);
            SettingService.SaveChatMessages(Messages.ToList());
        }
    }

    private async void ShowReference(ChatMessageModel messageModel)
    {
        if (MessageHelper.IsShowing) MessageHelper.Close();

        var verses = new List<VerseMessageModel>();
        foreach (var surah in messageModel.Reference)
        foreach (var verseWithResult in surah.ToVerses())
            verses.Add(new VerseMessageModel(surah, verseWithResult, verseWithResult.ToString()));

        _control = new VerseMessageControl(verses);

        MessageHelper.ShowMessage("Reference", _control, false);
    }
}