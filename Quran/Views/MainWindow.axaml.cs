using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Views.Pages;

namespace Quran.Views;

public partial class MainWindow : Window
{
    private AView? CurrentPage { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        ShowPage("Home");
        EnableNavButtons("Home");
    }

    private void NavButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        var pageName = button.Tag?.ToString();

        if (string.IsNullOrWhiteSpace(pageName))
            return;

        ShowPage(pageName);


    }

    private void ShowPage(string pageName, object? parameter = null)
    {
        if(CurrentPage != null)
        {
            CurrentPage.GotoPageRequested -= ShowPage;
        }
        CurrentPage = pageName switch
        {
            "Home" => new HomeView(),
            "Quran" => new QuranView(),
            "Bookmarks" => new BookmarksView(),
            "Search" => new SearchView(),
            "Settings" => new SettingsView(),

            _ => new HomeView()
        };
        
        CurrentPage.Load(parameter);
        MainContent.Content = CurrentPage;
        CurrentPage.GotoPageRequested += ShowPage;
        EnableNavButtons(pageName);
    }

    private void EnableNavButtons(string pageName = "")
    {
        var currentButton = pageName switch
        {
            "Home" => NavHomeButton,
            "Quran" => NavQuranButton,
            "Bookmarks" => NavBookmarksButton,
            "Search" => NavSearchButton,
            "Settings" => NavSettingsButton,

            _ => NavHomeButton
        };
        
        NavHomeButton.IsEnabled = true;
        NavQuranButton.IsEnabled = true;
        NavBookmarksButton.IsEnabled = true;
        NavSearchButton.IsEnabled = true;
        NavSettingsButton.IsEnabled = true;
        currentButton.IsEnabled = false;
    }
}