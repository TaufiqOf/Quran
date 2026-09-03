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
    private string _content = string.Empty;
    private bool _asReference = false;
    private Timer _timer = new Timer();
    private DateTime _time;
    public DateTime Time{ get => _time; set => _time = value; }
    public ChatMessageModel()
    {
        _timer.Elapsed+= (s, e) => OnPropertyChanged(nameof(TimeString));
        _timer.Interval = 1000;
        _timer.Start();
    }
    public string TimeString => (DateTime.Now-_time).Humanize(precision: 1, minUnit: Humanizer.TimeUnit.Second);
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