using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Quran.Models;

namespace Quran.Views.Component;

public partial class VerseComponent : UserControl
{
    public Verse Verse { get; }

    public delegate void VerseSelectedEventHandler(Verse verse);
    public event VerseSelectedEventHandler? VerseSelected;
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<VerseComponent, bool>(
            nameof(IsSelected),
            false);

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public VerseComponent(Surah surah, SurahSynopsis? synopsis)
    {
        InitializeComponent();
    }

    public VerseComponent(Verse verse)
    {
        Verse = verse;
        InitializeComponent();

        TextBlockArabic.Text = verse.Text;
        TextBlockTranslation.Text = verse.Translation;
        TextBlockTransliteration.Text = verse.Transliteration;
        TextBlockVerseNumber.Text = $"Verse {verse.Id}";
    }

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsSelectedProperty)
        {
            UpdateSelectedState();
        }
    }

    private void UpdateSelectedState()
    {
        if (IsSelected)
        {
            VerseCard.Classes.Add("selected");
        }
        else
        {
            VerseCard.Classes.Remove("selected");
        }
    }

    private void ButtonBookmark_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
    }

    private void ButtonPlay_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
    }

    private void VerseCard_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        OnVerseSelected(Verse);
    }

    protected virtual void OnVerseSelected(Verse verse)
    {
        VerseSelected?.Invoke(verse);
    }
}