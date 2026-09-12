using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Quran.Models;

namespace Quran.Helpers;

public static class SettingService
{
    private static AppSettings? _appSettings;

    public static readonly string SettingsFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "quran_settings.json");

    public static readonly string ChatModelSettingsFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "quran_chat_model_settings.json");

    public static void SaveLanguagePreference(string languageCode)
    {
        var appSettings = LoadAppSettings();
        appSettings.Language = languageCode;
        SaveSettings(appSettings);
    }
    public static void SaveCopySurahStructurePreference(string copySurahStructure)
    {
        var appSettings = LoadAppSettings();
        appSettings.CopySurahStructure = copySurahStructure;
        SaveSettings(appSettings);
    }
    public static void SaveCopyVerseStructurePreference(string copyVerseStructure)
    {
        var appSettings = LoadAppSettings();
        appSettings.CopyVerseStructure = copyVerseStructure;
        SaveSettings(appSettings);
    }
    public static void SaveReaderModePreference(string readerMode)
    {
        var appSettings = LoadAppSettings();
        appSettings.ReaderMode = readerMode;
        SaveSettings(appSettings);
    }

    public static void SaveChatMessages(List<ChatMessageModel> chatMessages)
    {
        SaveSettings(chatMessages);
    }

    public static void SaveAiSettings(AiSettings aiSettings)
    {
        var appSettings = LoadAppSettings();
        appSettings.AiSettings = aiSettings;
        SaveSettings(appSettings);
    }


    public static List<ChatMessageModel> LoadChatMessages()
    {
        var settings = LoadChatModelSettings();
        return settings.ChatMessages;
    }

    public static string? LoadCopySurahStructurePreference()
    {
        var settings = LoadAppSettings();
        return settings.CopySurahStructure;
    }
    public static string? LoadCopyVerseStructurePreference()
    {
        var settings = LoadAppSettings();
        return settings.CopyVerseStructure;
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

    public static AiSettings LoadAiSettings()
    {
        var settings = LoadAppSettings();
        return settings.AiSettings;
    }


    private static AppSettings LoadAppSettings()
    {
        try
        {
            if(_appSettings != null) return _appSettings;
            if (!File.Exists(SettingsFilePath)) return new AppSettings();
            var json = File.ReadAllText(SettingsFilePath);
            _appSettings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            return _appSettings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    private static ChatModelSettings LoadChatModelSettings()
    {
        try
        {
            if (!File.Exists(ChatModelSettingsFilePath)) return new ChatModelSettings();
            var json = File.ReadAllText(ChatModelSettingsFilePath);
            return JsonConvert.DeserializeObject<ChatModelSettings>(json) ?? new ChatModelSettings();
        }
        catch
        {
            return new ChatModelSettings();
        }
    }

    public static void SaveSettings(AppSettings? appSettings)
    {
        if (appSettings != null)
        {
            SaveAppSettings(appSettings);
        }
    }
    
    public static void SaveSettings(List<ChatMessageModel>? chatMessages)
    {
        if (chatMessages != null)
        {
            var chatModelSettings = LoadChatModelSettings();

            // Remove messages that no longer exist in the incoming list
            chatModelSettings.ChatMessages = chatModelSettings.ChatMessages
                .Where(existing => 
                    chatMessages.Any(incoming => 
                        incoming.Id == existing.Id))
                .ToList();

            // Add new messages
            var existingIds = chatModelSettings.ChatMessages
                .Select(x => x.Id)
                .ToHashSet();

            var newMessages = chatMessages
                .Where(incoming => 
                    !existingIds.Contains(incoming.Id))
                .ToList();

            chatModelSettings.ChatMessages.AddRange(newMessages);

            SaveChatModelSettings(chatModelSettings);
        }

    }

    private static string SaveSettings(string? language, string? readerMode,
        AiSettings? aiSettings, string? copySurahStructure, string? copyVerseStructure)
    {
        var settings = LoadAppSettings();
        settings.Language = language ?? settings.Language;
        settings.ReaderMode = readerMode ?? settings.ReaderMode;
        settings.AiSettings = aiSettings ?? settings.AiSettings;
        settings.CopySurahStructure = copySurahStructure ?? settings.CopySurahStructure;
        settings.CopyVerseStructure = copyVerseStructure ?? settings.CopyVerseStructure;
        SaveAppSettings(settings);
        return "Settings saved successfully.";
    }

    private static void SaveAppSettings(AppSettings settings)
    {
        var json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(SettingsFilePath, json);
    }

    private static void SaveChatModelSettings(ChatModelSettings settings)
    {
        var json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(ChatModelSettingsFilePath, json);
    }


    public static void SaveCurrentPositionSettings(int currentSurahId, int? currentVerseId)
    {
        var appsettings = LoadAppSettings();
        appsettings.CurrentSurahId = currentSurahId;
        appsettings.CurrentVerseId = currentVerseId;
        SaveAppSettings(appsettings);
    }

    public static (int surahId, int? verseId) LoadCurrentPosition()
    {
        var appSettings = LoadAppSettings();
        return (appSettings.CurrentSurahId, appSettings.CurrentVerseId);
    }
}