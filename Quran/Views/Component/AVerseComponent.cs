using Avalonia;
using Avalonia.Controls;
using Quran.Models;

namespace Quran.Views.Component;

public abstract class AVerseComponent : UserControl
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<VerseComponent, bool>(
            nameof(VerseComponent.IsSelected),
            false);

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
    public Verse? Verse { get; protected set; }

    public delegate void VerseSelectedEventHandler(Verse verse);

    public event VerseSelectedEventHandler? VerseSelected;
    public abstract void UpdateSelectedState();
    
    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty)
        {
            UpdateSelectedState();
        }
    }
    
    protected void OnVerseSelected(Verse? verse)
    {
        if(verse == null) return;
        this.VerseSelected?.Invoke(verse);
    }
}