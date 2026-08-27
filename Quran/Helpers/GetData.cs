using System.Collections.Generic;
using Quran.Models;

namespace Quran.Helpers;

public static class GetData
{
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