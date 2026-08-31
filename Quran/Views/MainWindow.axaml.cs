using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Helpers;
using Quran.Views.Pages;

namespace Quran.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += async (_, _) =>
        {
            ShowPage("Home");
            EnableNavButtons("Home");
        };
    }

    private AView? CurrentPage { get; set; }
    private Dictionary<string, AView> Pages { get; } = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SearchManager.RegisterSearcher();
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
        if (CurrentPage != null) CurrentPage.GotoPageRequested -= ShowPage;

        Pages.TryGetValue(pageName, out var currentPage);
        if (currentPage != null)
        {
            CurrentPage = currentPage;
        }
        else
        {
            CurrentPage = pageName switch
            {
                "Home" => new HomeView(),
                "Quran" => new QuranView(),
                "Bookmarks" => new BookmarksView(),
                "Search" => new SearchView(),
                "Settings" => new SettingsView(),

                _ => new HomeView()
            };
            CurrentPage.ReloadRequested += ReloadRequested;
            Pages.Add(pageName, CurrentPage);
        }

        CurrentPage.Load(parameter);
        MainContent.Content = CurrentPage;
        CurrentPage.GotoPageRequested += ShowPage;
        EnableNavButtons(pageName);
    }

    private void ReloadRequested()
    {
        foreach (var keyValuePair in Pages)
        {
            keyValuePair.Value.Reload();
        }
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