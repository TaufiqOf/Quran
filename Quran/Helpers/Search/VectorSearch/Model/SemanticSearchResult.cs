using System.Collections.Generic;

namespace Quran.Helpers.Search.VectorSearch.Model;

public class SemanticSearchResult
{
    public int SurahId { get; set; }

    public int VerseId { get; set; }

    public double Score { get; set; }

    public string Reference =>
        $"{SurahId}:{VerseId}";

    public List<WordMappingResult> Impacts { get; set; }
}