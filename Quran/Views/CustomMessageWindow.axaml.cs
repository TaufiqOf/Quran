using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Quran.Views;

/// <summary>
/// A reusable application message dialog that can display optional details.
/// </summary>
public partial class CustomMessageWindow : Window
{
    public static readonly StyledProperty<string> HeadingProperty =
        AvaloniaProperty.Register<CustomMessageWindow, string>(nameof(Heading), "Message");

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<CustomMessageWindow, string>(nameof(Message), string.Empty);
    

    public CustomMessageWindow()
    {
        InitializeComponent();
    }

    public CustomMessageWindow(string title, string message)
        : this()
    {
        Title = title;
        Heading = title;
        Message = message;
    }

    public string Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
