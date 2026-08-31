using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Helpers;

namespace Quran.Views.Pages;

public partial class SettingsView : AView
{
  
    public SettingsView()
    {
        InitializeComponent();
    }

    public override async Task Load(params object?[] parameter)
    {
        var languageCode = DataManager.LoadLanguagePreference();
        SelectLanguage(languageCode);
        await Task.CompletedTask;
    }

    public override async Task Reload(params object?[] parameter)
    {
        await Load(parameter);
    }


    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedLanguage = GetSelectedLanguageCode();
        if (string.IsNullOrWhiteSpace(selectedLanguage))
        {
            MessageHelper.ShowMessage("Settings", "Please select a language.");
            return;
        }

        DataManager.SaveLanguagePreference(selectedLanguage);
        DataManager.LoadSurahs(selectedLanguage);
        ReloadRequested?.Invoke();
        MessageHelper.ShowMessage("Settings Saved", "Your language preference has been saved.");
    }

    private string GetSelectedLanguageCode()
    {
        if (LanguageComboBox.SelectedItem is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string languageCode)
        {
            return languageCode;
        }

        return "en";
    }

    private void SelectLanguage(string languageCode)
    {
        for (var i = 0; i < LanguageComboBox.ItemCount; i++)
        {
            if (LanguageComboBox.Items[i] is ComboBoxItem item && item.Tag is string code &&
                string.Equals(code, languageCode, StringComparison.OrdinalIgnoreCase))
            {
                LanguageComboBox.SelectedIndex = i;
                return;
            }
        }

        LanguageComboBox.SelectedIndex = 0;
    }

    

  
}