using System.Collections.Generic;
using System.Text.Json.Serialization;
using Quran.Helpers.Search.VectorSearch;

namespace Quran.Models;

public class Verse
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;

    [JsonPropertyName("translation")] public string Translation { get; set; } = string.Empty;

    [JsonPropertyName("transliteration")] public string Transliteration { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"({Id}){Text}\n{Translation}";
    }
}

public class VerseResult : Verse
{
    public double? SimilarityScore { get; set; }
    public List<WordMappingResult> Impacts { get; set; }
}