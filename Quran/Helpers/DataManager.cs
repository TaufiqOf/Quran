using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Quran.Models;

namespace Quran.Helpers;

public static class DataManager
{
    private static string DataPath => Path.Combine(AppContext.BaseDirectory, "Data");

    private static readonly string BookmarkFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmarks.json");

    private static readonly string SettingsFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "quran_settings.json");

    static DataManager()
    {
        if (!File.Exists(BookmarkFilePath)) File.WriteAllText(BookmarkFilePath, "[]");
        // Load Surahs and Surah Orders on initialization
        Bookmarks = GetBookmarks();
        LoadSurahs(LoadLanguagePreference());
    }

    public static List<SurahResult> SurahResults
    {
        get
        {
            var surahResults = Surahs.Select(originalSurah => new SurahResult
            {
                Id = originalSurah.Id,
                Name = originalSurah.Name,
                Transliteration = originalSurah.Transliteration,
                Translation = originalSurah.Translation,
                Type = originalSurah.Type,
                TotalVerses = originalSurah.TotalVerses,
                Verses = originalSurah.Verses,
                VerseResults = originalSurah.Verses.Select(q=> new VerseResult()
                {
                    Id = q.Id,
                    Text = q.Text,
                    Translation = q.Translation,
                    Transliteration = q.Transliteration,
                }).ToList()
            });
            return surahResults.ToList();
        }
    }

    public static List<Surah> Surahs { get; private set; } = new();
    public static List<SurahOrder> SurahOrders { get; private set; } = new();
    public static List<SurahSynopsis> SurahSynopses { get; private set; } = new();

    public static Surah? CurrentSurah { get; set; }
    public static int? CurrentVerseId { get; set; }

    public static List<Bookmark> Bookmarks { get; }

    public static void LoadSurahs(string language = "en")
    {
        Surahs = GetSurahs(language);
        SurahOrders = SurahOrder();
        SurahSynopses = SurahSynopsis();

        // Add transliterations to verses.
        foreach (var surahTransliteration in GetSurahTransliterations())
        {
            var surah = Surahs.FirstOrDefault(q => q.Id == surahTransliteration.Id);

            if (surah != null)
                foreach (var transliterationVerse in surahTransliteration.Verses)
                {
                    var verse = surah.Verses
                        .FirstOrDefault(v => v.Id == transliterationVerse.Id);

                    if (verse != null)
                        verse.Transliteration =
                            transliterationVerse.Transliteration;
                }
        }
    }

    public static List<Surah> GetSurahs(string language)
    {
        var json = JsonReader.ReadStringFromFile(Path.Combine(DataPath, $"quran_{language}.json"));
        return JsonReader.ReadJsonList<Surah>(json);
    }

    public static List<Surah> GetSurahTransliterations()
    {
        var json = JsonReader.ReadStringFromFile(Path.Combine(DataPath, "quran_transliteration.json"));
        return JsonReader.ReadJsonList<Surah>(json);
    }

    public static Surah GetSurahById(int surahId)
    {
        var json = JsonReader.ReadStringFromFile(Path.Combine(DataPath, "quran_en.json"));
        return JsonReader.ReadJson<Surah>(json) ?? new Surah();
    }

    public static List<SurahOrder> SurahOrder()
    {
        var json = JsonReader.ReadStringFromFile(Path.Combine(DataPath, "surah_order.json"));
        return JsonReader.ReadJsonList<SurahOrder>(json);
    }

    public static List<SurahSynopsis> SurahSynopsis()
    {
        var json = JsonReader.ReadStringFromFile(Path.Combine(DataPath, "surah_synopsis.json"));
        return JsonReader.ReadJsonList<SurahSynopsis>(json);
    }

    public static List<Bookmark> GetBookmarks()
    {
        var json = File.ReadAllText(BookmarkFilePath);
        return JsonReader.ReadJsonList<Bookmark>(json);
    }

    public static bool IsBookmarked(int surahId, int verseId)
    {
        return Bookmarks.Any(b => b.SurahId == surahId && b.VerseId == verseId);
    }

    public static void AddBookmark(Bookmark bookmark)
    {
        if (IsBookmarked(bookmark.SurahId, bookmark.VerseId)) return; // Bookmark already exists, do not add it again
        bookmark.Timestamp = DateTime.Now; // Update the timestamp to the current time
        Bookmarks.Add(bookmark);
        SaveBookmarks(Bookmarks);
    }

    public static void RemoveBookmark(Bookmark bookmark)
    {
        if (!IsBookmarked(bookmark.SurahId, bookmark.VerseId)) return; // Bookmark does not exist, do not remove it

        Bookmarks.RemoveAll(b => b.SurahId == bookmark.SurahId && b.VerseId == bookmark.VerseId);
        SaveBookmarks(Bookmarks);
    }

    public static void SaveBookmarks(List<Bookmark> bookmarks)
    {
        var json = JsonSerializer.Serialize(bookmarks);
        File.WriteAllText(BookmarkFilePath, json);
    }

    public static string LoadLanguagePreference()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return "en";

            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return string.IsNullOrWhiteSpace(settings?.Language) ? "en" : settings.Language;
        }
        catch
        {
            return "en";
        }
    }

    public static void SaveLanguagePreference(string languageCode)
    {
        var settings = LoadAppSettings();
        settings.Language = languageCode;
        SaveAppSettings(settings);
    }

    public static string LoadReaderModePreference()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return "Compact";

            var settings = LoadAppSettings();
            return string.IsNullOrWhiteSpace(settings?.ReaderMode) ? "Compact" : settings.ReaderMode;
        }
        catch
        {
            return "Compact";
        }
    }

    public static void SaveReaderModePreference(string readerMode)
    {
        var settings = LoadAppSettings();
        settings.ReaderMode = readerMode;
        SaveAppSettings(settings);
    }

    private static AppSettings LoadAppSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return new AppSettings();

            var json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private static void SaveAppSettings(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(SettingsFilePath, json);
    }
}