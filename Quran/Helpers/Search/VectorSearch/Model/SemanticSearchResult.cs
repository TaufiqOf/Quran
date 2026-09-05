using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Quran.Helpers.Search.VectorSearch.Model;

public class SemanticSearchResult
{
    [JsonPropertyName("surahId")]
    public int SurahId { get; set; }

    [JsonPropertyName("verseId")]
    public int VerseId { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
    
    [JsonPropertyName("bookmarked")]
    public bool Bookmarked { get; set; }

    public string Reference =>
        $"{SurahId}:{VerseId}";

    public List<WordMappingResult>? Impacts { get; set; }
}