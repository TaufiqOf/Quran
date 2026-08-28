using System;
using System.Text.Json.Serialization;

namespace Quran.Models;

public class SurahSynopsis
{
    [JsonPropertyName("surah_id")] public int SurahId { get; set; }
    [JsonPropertyName("synopsis")] public string Synopsis { get; set; } = string.Empty;
    [JsonPropertyName("themes")] public string[] Themes { get; set; } = Array.Empty<string>();
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
}