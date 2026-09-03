using System.Text.Json.Serialization;

namespace Quran.Models;

public class Tafsir
{
    [JsonPropertyName("text")] 
    public string Text { get; set; }
    [JsonPropertyName("ayah")] 
    public int VerseId { get; set; }
    [JsonPropertyName("surah")] 
    public int SurahId { get; set; }
}

