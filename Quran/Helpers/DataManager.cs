using System.Collections.Generic;
using System.Linq;
using Quran.Models;

namespace Quran.Helpers;

public static class DataManager
{
    public static List<Surah> Surahs { get; private set; }
    public static List<SurahOrder> SurahOrders { get; private set; }
    public static List<SurahSynopsis> SurahSynopses { get; private set; }

    public static Surah? CurrentSurah { get; set; }
    public static int? CurrentVerseIndex { get; set; }

    static DataManager()
    {
        // Load Surahs and Surah Orders on initialization
        Surahs = GetSurahs();
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

    public static List<Surah> GetSurahs()
    {
        var json = JsonReader.ReadJsonFromResource("quran_en.json");
        return JsonReader.ReadJsonList<Surah>(json) ?? new List<Surah>();
    }

    public static List<Surah> GetSurahTransliterations()
    {
        var json = JsonReader.ReadJsonFromResource("quran_transliteration.json");
        return JsonReader.ReadJsonList<Surah>(json) ?? new List<Surah>();
    }

    public static Surah GetSurahById(int surahId)
    {
        var json = JsonReader.ReadJsonFromResource("quran_en.json");
        return JsonReader.ReadJson<Surah>(json) ?? new Surah();
    }

    public static List<SurahOrder> SurahOrder()
    {
        var json = JsonReader.ReadJsonFromResource("surah_order.json");
        return JsonReader.ReadJsonList<SurahOrder>(json);
    }

    public static List<SurahSynopsis> SurahSynopsis()
    {
        var json = JsonReader.ReadJsonFromResource("surah_synopsis.json");
        return JsonReader.ReadJsonList<SurahSynopsis>(json);
    }
}