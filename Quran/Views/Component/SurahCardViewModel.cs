using System.ComponentModel;
using System.Runtime.CompilerServices;
using Quran.Models;

namespace Quran.Views.Component;

public class SurahCardViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public Surah Surah { get; }
    public SurahSynopsis? Synopsis { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public SurahCardViewModel(Surah surah, SurahSynopsis? synopsis)
    {
        Surah = surah;
        Synopsis = synopsis;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}