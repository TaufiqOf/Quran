using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Pages;

public partial class SettingsView : AView
{
    public SettingsView()
    {
        InitializeComponent();
        var translator = DataManager.Translators();
        var comboBoxItems = translator.Select(t => new ComboBoxItem
        {
            Content = $"{t.Language} ({t.Name})",
            Tag = t.Id
        }).ToList();
        comboBoxItems.ForEach(item => LanguageComboBox.Items.Add(item));
    }

    public override async Task Load(params object?[] parameter)
    {
        var readerMode = SettingService.LoadReaderModePreference();
        SelectReaderMode(readerMode);

        var aiSettings = SettingService.LoadAiSettings();
        SelectAiSettings(aiSettings);


        var languageCode = SettingService.LoadLanguagePreference();
        SelectLanguage(languageCode);
        await Task.CompletedTask;
    }

    private void SelectAiSettings(AiSettings aiSettings)
    {
        AiProviderComboBox.SelectedIndex = 0;
        for (var i = 0; i < AiProviderComboBox.ItemCount; i++)
            if (AiProviderComboBox.Items[i] is ComboBoxItem item && item.Tag is string model &&
                string.Equals(model, aiSettings.Provider.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                AiProviderComboBox.SelectedIndex = i;
                break;
            }

        AiModelModelTextBox.Text = aiSettings.Model;
        AiModelApiKeyTextBox.Text = aiSettings.Endpoint;
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

        SettingService.SaveLanguagePreference(selectedLanguage);
        var selectedAiProviderItem = AiProviderComboBox.SelectedItem as ComboBoxItem;
        SettingService.SaveReaderModePreference(selectedReaderMode);
        var aiSettings = new AiSettings
        {
            Provider = Enum.TryParse<AiProvider>(selectedAiProviderItem?.Tag as string, out var provider)
                ? provider
                : AiProvider.Ollama,
            Model = AiModelModelTextBox.Text ?? string.Empty,
            Endpoint = AiModelApiKeyTextBox.Text ?? string.Empty
        };
        SettingService.SaveAiSettings(aiSettings);
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