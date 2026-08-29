using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Quran.Models;

namespace Quran.Helpers;

public static class DataManager
{
    private static readonly string bookmarkFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmarks.json");

    static DataManager()
    {
        if (!File.Exists(bookmarkFilePath)) File.WriteAllText(bookmarkFilePath, "[]");
        // Load Surahs and Surah Orders on initialization
        Surahs = GetSurahs();
        SurahOrders = SurahOrder();
        SurahSynopses = SurahSynopsis();
        Bookmarks = GetBookmarks();

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

    public static List<Surah> Surahs { get; }
    public static List<SurahOrder> SurahOrders { get; private set; }
    public static List<SurahSynopsis> SurahSynopses { get; private set; }


    public static Surah? CurrentSurah { get; set; }
    public static int? CurrentVerseId { get; set; }

    public static List<Bookmark> Bookmarks { get; }

    public static List<Surah> GetSurahs()
    {
        var json = JsonReader.ReadStringFromResource("quran_en.json");
        return JsonReader.ReadJsonList<Surah>(json) ?? new List<Surah>();
    }

    public static List<Surah> GetSurahTransliterations()
    {
        var json = JsonReader.ReadStringFromResource("quran_transliteration.json");
        return JsonReader.ReadJsonList<Surah>(json) ?? new List<Surah>();
    }

    public static Surah GetSurahById(int surahId)
    {
        var json = JsonReader.ReadStringFromResource("quran_en.json");
        return JsonReader.ReadJson<Surah>(json) ?? new Surah();
    }

    public static List<SurahOrder> SurahOrder()
    {
        var json = JsonReader.ReadStringFromResource("surah_order.json");
        return JsonReader.ReadJsonList<SurahOrder>(json);
    }

    public static List<SurahSynopsis> SurahSynopsis()
    {
        var json = JsonReader.ReadStringFromResource("surah_synopsis.json");
        return JsonReader.ReadJsonList<SurahSynopsis>(json);
    }

    public static List<Bookmark> GetBookmarks()
    {
        var json = File.ReadAllText(bookmarkFilePath);
        return JsonReader.ReadJsonList<Bookmark>(json) ?? new List<Bookmark>();
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
        File.WriteAllText(bookmarkFilePath, json);
    }
}