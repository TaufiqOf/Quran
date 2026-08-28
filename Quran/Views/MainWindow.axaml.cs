using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Views.Pages;

namespace Quran.Views;

public partial class MainWindow : Window
{
    private AView? CurrentPage { get; set; }
    private Dictionary<string, AView> Pages { get; set; } = new Dictionary<string, AView>();

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
        if (this.CurrentPage != null)
        {
            this.CurrentPage.GotoPageRequested -= ShowPage;
        }

        this.Pages.TryGetValue(pageName, out var currentPage);
        if (currentPage != null)
        {
            this.CurrentPage = currentPage;
        }
        else
        {
            this.CurrentPage = pageName switch
            {
                "Home" => new HomeView(),
                "Quran" => new QuranView(),
                "Bookmarks" => new BookmarksView(),
                "Search" => new SearchView(),
                "Settings" => new SettingsView(),

                _ => new HomeView()
            };
            this.Pages.Add(pageName, this.CurrentPage);
        }

        this.CurrentPage.Load(parameter);
        MainContent.Content = this.CurrentPage;
        this.CurrentPage.GotoPageRequested += ShowPage;
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