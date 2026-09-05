using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Humanizer;

namespace Quran.Models;

public class ChatMessageModel : INotifyPropertyChanged
{
    private readonly Stopwatch _stopwatch;
    private readonly Timer _timer = new();
    private string _content = string.Empty;
    private bool _isWorking;

    public ChatMessageModel()
    {
        _timer.Elapsed += (s, e) =>
        {
            if(!IsWorking)
                return;
            ResponseTime = _stopwatch?.Elapsed ?? TimeSpan.Zero;
            OnPropertyChanged(nameof(TimeString));
        };
        _timer.Interval = 1000;
        _stopwatch = Stopwatch.StartNew();
        _timer.Start();

        if (IsWorking)
            _stopwatch.Start();
        else
            _stopwatch.Stop();
        Reference = new List<SurahResult>();
    }

    public TimeSpan ResponseTime
    {
        get;
        set
        {
            if (value.Equals(field)) return;
            field = value;
            OnPropertyChanged(nameof(HumanizedResponseTime));
            OnPropertyChanged();
        }
    }

    public string HumanizedResponseTime =>
        ResponseTime.Humanize();

    public bool IsWorking
    {
        get => _isWorking;
        set
        {
            if (_isWorking != value)
            {
                _isWorking = value;
                if (!_isWorking)
                    _stopwatch.Stop();
                else
                    _stopwatch.Start();

                OnPropertyChanged();
            }
        }
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
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public List<SurahResult> Reference
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            field = value;
            HasReference = value?.Any() ?? false;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}