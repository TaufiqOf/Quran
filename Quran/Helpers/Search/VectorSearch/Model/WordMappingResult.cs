using System.Text.Json.Serialization;

namespace Quran.Helpers.Search.VectorSearch.Model;

public class WordMappingResult
{
    [JsonPropertyName("queryWord")]
    public string? QueryWord { get; set; }
    [JsonPropertyName("verseWord")]
    public string VerseWord { get; set; } = string.Empty;
    [JsonPropertyName("correlationScore")]
    public float CorrelationScore { get; set; }
}