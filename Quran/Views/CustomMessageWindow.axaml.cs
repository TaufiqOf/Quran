using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Quran.Views;

/// <summary>
///     A reusable application message dialog that can display optional details.
/// </summary>
public partial class CustomMessageWindow : Window
{
    public static readonly StyledProperty<string> HeadingProperty =
        AvaloniaProperty.Register<CustomMessageWindow, string>(nameof(Heading));

    public static readonly StyledProperty<UserControl> UserContentControlProperty =
        AvaloniaProperty.Register<CustomMessageWindow, UserControl>(nameof(UserContentControl));


    public CustomMessageWindow(string title)
    {
        InitializeComponent();
        Heading = title;
        Title = title;
    }


    public string Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public UserControl UserContentControl
    {
        get => GetValue(UserContentControlProperty);
        set => SetValue(UserContentControlProperty, value);
    }

    public void SetControl(UserControl content)
    {
        ContentControl.Content = content;
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}