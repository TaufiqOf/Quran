using System;
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

        var readerMode = DataManager.LoadReaderModePreference();
        SelectReaderMode(readerMode);
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

        var selectedReaderMode = GetSelectedReaderMode();
        if (string.IsNullOrWhiteSpace(selectedReaderMode))
        {
            MessageHelper.ShowMessage("Settings", "Please select a reader view.");
            return;
        }

        DataManager.SaveLanguagePreference(selectedLanguage);
        DataManager.SaveReaderModePreference(selectedReaderMode);
        DataManager.LoadSurahs(selectedLanguage);
        ReloadRequested?.Invoke();
        MessageHelper.ShowMessage("Settings Saved", "Your preferences have been saved.");
    }

    private string GetSelectedLanguageCode()
    {
        if (LanguageComboBox.SelectedItem is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string languageCode)
            return languageCode;

        return "en";
    }

    private void SelectLanguage(string languageCode)
    {
        for (var i = 0; i < LanguageComboBox.ItemCount; i++)
            if (LanguageComboBox.Items[i] is ComboBoxItem item && item.Tag is string code &&
                string.Equals(code, languageCode, StringComparison.OrdinalIgnoreCase))
            {
                LanguageComboBox.SelectedIndex = i;
                return;
            }

        LanguageComboBox.SelectedIndex = 0;
    }

    private string GetSelectedReaderMode()
    {
        if (ReaderModeComboBox.SelectedItem is ComboBoxItem comboBoxItem && comboBoxItem.Tag is string readerMode)
            return readerMode;

        return "Compact";
    }

    private void SelectReaderMode(string readerMode)
    {
        for (var i = 0; i < ReaderModeComboBox.ItemCount; i++)
            if (ReaderModeComboBox.Items[i] is ComboBoxItem item && item.Tag is string mode &&
                string.Equals(mode, readerMode, StringComparison.OrdinalIgnoreCase))
            {
                ReaderModeComboBox.SelectedIndex = i;
                return;
            }

        ReaderModeComboBox.SelectedIndex = 0;
    }
}