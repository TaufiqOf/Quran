using System.Text.Json.Serialization;

namespace Quran.Models;

public class Tafsir
{
    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;

    [JsonPropertyName("ayah")] public int VerseId { get; set; } = 0;

    [JsonPropertyName("surah")] public int SurahId { get; set; } = 0;
}