using System.Text.Json.Serialization;

namespace Quran.Models;

public class Surah
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("transliteration")]
    public string Transliteration { get; set; } = string.Empty;

    [JsonPropertyName("translation")]
    public string Translation { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("total_verses")]
    public int TotalVerses { get; set; }

    [JsonPropertyName("verses")]
    public Verse[] Verses { get; set; } = [];
}

