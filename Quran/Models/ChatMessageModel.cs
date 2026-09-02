using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Quran.Models;

public class ChatMessageModel : INotifyPropertyChanged
{
    private string _content = string.Empty;
    private bool _asReference = false;

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