using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Views.Pages;

namespace Quran.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ShowPage("Home");

        NavHomeButton.IsEnabled = false;
    }

    private void NavButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        var pageName = button.Tag?.ToString();

        if (string.IsNullOrWhiteSpace(pageName))
            return;

        ShowPage(pageName);

        EnableNavButtons();

        button.IsEnabled = false;
    }

    private void ShowPage(string pageName)
    {
        AView page = pageName switch
        {
            "Home" => new HomeView(),
            "Quran" => new QuranView(),
            "Bookmarks" => new BookmarksView(),
            "Search" => new SearchView(),
            "Settings" => new SettingsView(),

            _ => new HomeView()
        };
        page.Load();
        MainContent.Content = page;
    }

    private void EnableNavButtons()
    {
        NavHomeButton.IsEnabled = true;
        NavQuranButton.IsEnabled = true;
        NavBookmarksButton.IsEnabled = true;
        NavSearchButton.IsEnabled = true;
        NavSettingsButton.IsEnabled = true;
    }
}