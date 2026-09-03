using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Humanizer;

namespace Quran.Models;

public class ChatMessageModel : INotifyPropertyChanged
{
    private readonly Timer _timer = new();
    private bool _asReference;
    private string _content = string.Empty;

    public ChatMessageModel()
    {
        _timer.Elapsed += (s, e) => OnPropertyChanged(nameof(TimeString));
        _timer.Interval = 1000;
        _timer.Start();
    }

    public DateTime Time { get; set; }

    public string TimeString => (DateTime.Now - Time).Humanize(minUnit: TimeUnit.Second);
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
                OnPropertyChanged(nameof(HasReference));
            }
        }
    }

    public bool HasReference
    {
        get => _asReference;
        set
        {
            if (_asReference != value)
            {
                _asReference = value;
                OnPropertyChanged();
            }
        }
    }

    public List<SurahResult> Refference
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            field = value;
            HasReference = value != null && value.Any();
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}