using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component;

public partial class AudioComponent : UserControl
{
    public event Action? PlayAction;
    public event Action? PauseAction;
    public event Action<double>? SeekAction;
    public AudioComponent()
    {
        InitializeComponent();
    }
    
    private void PlayButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        PlayAction?.Invoke();
        PlayButton.IsEnabled = false;
        PauseButton.IsEnabled = true;
    }

    private void PauseButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        PauseAction?.Invoke();
        PlayButton.IsEnabled = true;
        PauseButton.IsEnabled = false;
    }

    private void ProgressSlider_OnValueChanged(
        object? sender,
        RangeBaseValueChangedEventArgs e)
    {
        SeekAction?.Invoke(e.NewValue);
    }
}