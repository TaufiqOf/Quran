using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Quran.Models;

namespace Quran.Helpers;

public static class SettingService
{
    public static readonly string SettingsFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "quran_settings.json");


    public static void SaveLanguagePreference(string languageCode)
    {
        SaveSettings(null, languageCode, null);
    }

    public static void SaveReaderModePreference(string readerMode)
    {
        SaveSettings(null, null, readerMode);
    }

    public static void SaveChatMessages(List<ChatMessageModel> chatMessages)
    {
        SaveSettings(chatMessages, null, null);
    }


    public static List<ChatMessageModel> LoadChatMessages()
    {
        var settings = LoadAppSettings();
        return settings.ChatMessages;
    }

    public static string LoadLanguagePreference()
    {
        var settings = LoadAppSettings();
        return settings.Language;
    }

    public static string LoadReaderModePreference()
    {
        var settings = LoadAppSettings();
        return settings.ReaderMode;
    }

    private static AppSettings LoadAppSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return new AppSettings();
            var json = File.ReadAllText(SettingsFilePath);
            return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private static string SaveSettings(List<ChatMessageModel>? chatMessages, string? language, string? readerMode)
    {
        var settings = LoadAppSettings();
        settings.ChatMessages = chatMessages ?? settings.ChatMessages;
        settings.Language = language ?? settings.Language;
        settings.ReaderMode = readerMode ?? settings.ReaderMode;
        SaveAppSettings(settings);
        return "Settings saved successfully.";
    }

    private static void SaveAppSettings(AppSettings settings)
    {
        var json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(SettingsFilePath, json);
    }
}