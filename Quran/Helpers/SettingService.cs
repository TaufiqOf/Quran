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
    public static readonly string ChatModelSettingsFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "quran_chat_model_settings.json");

    public static void SaveLanguagePreference(string languageCode)
    {
        SaveSettings(null, languageCode, null, null);
    }

    public static void SaveReaderModePreference(string readerMode)
    {
        SaveSettings(null, null, readerMode,null);
    }

    public static void SaveChatMessages(List<ChatMessageModel> chatMessages)
    {
        SaveSettings(chatMessages, null, null,null);
    }
    
    public static void SaveAiSettings(AiSettings aiSettings)
    {
        SaveSettings(null, null, null, aiSettings);
    }


    public static List<ChatMessageModel> LoadChatMessages()
    {
        var settings = LoadChatModelSettings();
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
    
    public static AiSettings LoadAiSettings()
    {
        var settings = LoadAppSettings();
        return settings.AiSettings;
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
    public static string SaveSettings(List<ChatMessageModel>? chatMessages, string? language, string? readerMode,
        AiSettings? aiSettings)
    {
        if (chatMessages != null)
        {
            var chatModelSettings = LoadChatModelSettings();
            chatModelSettings.ChatMessages = chatMessages;
            SaveChatModelSettings(chatModelSettings);
            return "Chat messages saved successfully.";
        }
        else
        {
            return SaveSettings(language, readerMode, aiSettings);
        }
    }
    private static string SaveSettings(string? language, string? readerMode,
        AiSettings? aiSettings)
    {
        var settings = LoadAppSettings();
        settings.Language = language ?? settings.Language;
        settings.ReaderMode = readerMode ?? settings.ReaderMode;
        settings.AiSettings = aiSettings ?? settings.AiSettings;
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
}