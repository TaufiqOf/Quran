namespace Quran.Helpers.Search.VectorSearch;

public class WordMappingResult
{
    public string? QueryWord { get; set; }
    public string VerseWord { get; set; } = string.Empty;
    public float CorrelationScore { get; set; }
}