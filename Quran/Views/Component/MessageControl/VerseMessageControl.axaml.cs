using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component.MessageControl;

public class VerseMessageModel : INotifyPropertyChanged
{
    private bool _isExpanded;

    public VerseMessageModel(Surah surah, Verse verse, string message)
    {
        Surah = surah;
        Verse = verse;
        Message = message;
    }

    public Surah Surah { get; set; }
    public Verse Verse { get; set; }
    public string Message { get; set; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class VerseMessageControl : UserControl
{
    private readonly List<VerseMessageModel> _messages;

    public VerseMessageControl(List<VerseMessageModel> messages)
    {
        InitializeComponent();
        _messages = messages ?? new List<VerseMessageModel>();

        if (_messages.Count > 0) _messages[0].IsExpanded = true;

        MessagesItemsControl.ItemsSource = _messages;
    }

    private void Expander_OnExpanded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Expander { DataContext: VerseMessageModel current }) return;

        foreach (var item in _messages)
            if (!ReferenceEquals(item, current))
                item.IsExpanded = false;
    }

    private async void CopyButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var contextText = string.Join("\n",
            _messages.Select(q=>q.Verse.Translation));
        var topLevel = TopLevel.GetTopLevel(this);
        var clipboard = topLevel?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(contextText);
        }
    }
}