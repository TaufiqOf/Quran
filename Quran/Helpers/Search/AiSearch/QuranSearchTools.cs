using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
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

    [Description(
        "Searches for exact occurrences of specific names, words, or entities in the Quran (e.g., 'Muhammad', 'Jesus', 'Pharaoh').")]
    public async Task<List<VerseReference>> FindExactWordMatchesAsync(
        [Description("The exact name or word to find in Arabic or English.")]
        string term,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cleanTerm = StripExistingLimit(term);
        var formattedTerm = $"{cleanTerm}:-1";

        var results = await _exactSearch.PerformSearch(formattedTerm, cancellationToken);
        return FlattenSurahResults(results);
    }

    [Description(
        "Searches for conceptual narratives, events, stories, or themes in the Quran (e.g., 'birth of Jesus', 'night of power', 'creation of heavens').")]
    public async Task<List<VerseReference>> FindTopicOrNarrativeAsync(
        [Description("The conceptual query or narrative subject.")]
        string topic,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cleanTopic = StripExistingLimit(topic);
        var formattedTopic = $"{cleanTopic}:-1";

        var results = await _semanticSearch.PerformSearch(formattedTopic, cancellationToken);
        return FlattenSurahResults(results);
    }

    private static string StripExistingLimit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var match = Regex.Match(text.Trim(), @":(-?\d+)$");
        return match.Success ? text.Substring(0, match.Index).Trim() : text.Trim();
    }

    private static List<VerseReference> FlattenSurahResults(List<SurahResult>? results)
    {
        if (results == null || results.Count == 0)
            return new List<VerseReference>();

        var list = new List<VerseReference>();

        foreach (var surah in results)
        {
            if (surah?.VerseResults == null)
                continue;

            foreach (var verse in surah.VerseResults)
                list.Add(new VerseReference
                {
                    SurahId = surah.Id,
                    VerseId = verse.Id,
                    Score = verse.SimilarityScore ?? surah.SimilarityScore ?? 0.0
                });
        }

        return list;
    }
}

public class VerseReference
{
    public int SurahId { get; set; }
    public int VerseId { get; set; }
    public double Score { get; set; }
}