using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Views;

namespace Quran;

public class App : Application
{
    private bool _isShowingUnhandledException;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        RequestedThemeVariant = ThemeVariant.Dark;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Dispatcher.UIThread.UnhandledException += OnUiThreadUnhandledException;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            MessageHelper.MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnUiThreadUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Keep the desktop lifetime alive after an exception that reaches the UI dispatcher.
        e.Handled = true;

        if (_isShowingUnhandledException)
            return;
        _isShowingUnhandledException = true;
        MessageHelper.ShowMessage("Unexpected error", e.Exception.ToString());
    }
}