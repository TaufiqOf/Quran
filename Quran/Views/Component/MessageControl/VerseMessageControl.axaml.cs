using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Helpers;
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
        _messages = messages;

        if (_messages.Count > 0) _messages[0].IsExpanded = true;

        MessagesItemsControl.ItemsSource = _messages;
        SurahComboBox.Items.Add(new ComboBoxItem()
        {
            Content = "All",
            Tag = "0"
        });
        _messages.Select(m => m.Surah).Distinct().ToList().Select(q=> new ComboBoxItem()
        {
            Content = q.ToString(),
            Tag = q.Id
        }).ToList().ForEach(item => SurahComboBox.Items.Add(item));
        SurahComboBox.SelectedIndex = 0;
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
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return;
        var text = string.Empty;
        foreach (var verseMessageModel in _messages)
        {
           text += verseMessageModel.Message + Environment.NewLine +
                   "________________________________________________" + Environment.NewLine + Environment.NewLine;
        }
        await CopyHelper.CopyClipboard(topLevel, text);
    }

    private void SurahComboBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        VerseComboBox.Items.Clear();
        VerseComboBox.Items.Add(new ComboBoxItem()
        {
            Content = "All",
            Tag = 0
        });
        var selectedSurahId = int.Parse((SurahComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "0");
        
        _messages
            .Where(m => m.Surah.Id == selectedSurahId)
            .ToList()
            .Select(q=> new ComboBoxItem()
            {
                Content = q.Verse.Id.ToString(),
                Tag = q.Verse.Id
            }).ToList().ForEach(item => VerseComboBox.Items.Add(item));
        MessagesItemsControl.ItemsSource = _messages
            .Where(m => selectedSurahId == 0 || m.Surah.Id == selectedSurahId).ToList();
        VerseComboBox.SelectedIndex = 0;
    }

    private void VerseComboBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedVerseId = int.Parse((VerseComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "0");
        var selectedSurahId = int.Parse((SurahComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "0");
        MessagesItemsControl.ItemsSource = _messages.Where(m => 
            (selectedVerseId == 0 || m.Verse.Id == selectedVerseId) && 
            (selectedSurahId == 0 || m.Surah.Id == selectedSurahId)).ToList();
    }
}