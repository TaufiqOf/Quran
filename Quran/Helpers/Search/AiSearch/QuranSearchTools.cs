using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search.AiSearch;

public class QuranSearchTools
{
    private readonly ISearch _exactSearch;
    private readonly ISearch _semanticSearch;

    public QuranSearchTools(ISearch exactSearch, ISearch semanticSearch)
    {
        _exactSearch = exactSearch;
        _semanticSearch = semanticSearch;
    }

    [Description("Searches for exact occurrences of specific names, words, or entities in the Quran (e.g., 'Muhammad', 'Jesus', 'Pharaoh').")]
    public async Task<List<VerseReference>> FindExactWordMatchesAsync(
        [Description("The exact name or word to find in Arabic or English.")] 
        string term,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = await _exactSearch.PerformSearch(term, cancellationToken);
        return FlattenSurahResults(results);
    }

    [Description("Searches for conceptual narratives, events, stories, or themes in the Quran (e.g., 'birth of Jesus', 'night of power', 'creation of heavens').")]
    public async Task<List<VerseReference>> FindTopicOrNarrativeAsync(
        [Description("The conceptual query or narrative subject.")] 
        string topic,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = await _semanticSearch.PerformSearch(topic, cancellationToken);
        return FlattenSurahResults(results);
    }

    private static List<VerseReference> FlattenSurahResults(List<SurahResult> surahs)
    {
        var references = new List<VerseReference>();
        foreach (var surah in surahs)
        {
            foreach (var verse in surah.VerseResults)
            {
                references.Add(new VerseReference
                {
                    SurahId = surah.Id,
                    VerseId = verse.Id,
                    Score = verse.SimilarityScore ?? 1.0
                });
            }
        }
        return references;
    }
}

public class VerseReference
{
    public int SurahId { get; set; }
    public int VerseId { get; set; }
    public double Score { get; set; }
}