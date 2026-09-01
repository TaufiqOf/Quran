using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search;

public class TextSearch : ISearch
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "about", "an", "and", "are", "as", "at", "be", "by", "for",
        "from", "how", "in", "is", "it", "of", "on", "or", "that", "the",
        "this", "to", "was", "what", "when", "where", "who", "will", "with"
    };

    public bool GetSearchMode(string searchText) => true;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task<List<SurahResult>> PerformSearch(string query, CancellationToken cancellationToken = default)
    {
        // 1. Initial cancellation check
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(new List<SurahResult>());

        int? topK = 200;

        // Extract :N modifier if present
        var match = Regex.Match(query, @":(\d+)$");
        if (match.Success)
        {
            topK = int.Parse(match.Groups[1].Value);
            query = query[..match.Index].Trim();
        }

        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(new List<SurahResult>());

        string rawPhrase = query.Trim();
        string normalizedQueryPhrase = StripPunctuation(rawPhrase);

        // 2. Tokenize and clean search terms
        var rawWords = Regex.Split(rawPhrase, @"\W+")
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToList();

        var searchTerms = rawWords
            .Where(w => !StopWords.Contains(w))
            .ToList();

        if (searchTerms.Count == 0) searchTerms = rawWords;

        // Flat collection to store every matching verse across all Surahs
        var flatVerseResults = new List<(Surah Surah, VerseResult VerseResult)>();

        // 3. Process Surahs & Verses
        foreach (var surah in DataManager.Surahs)
        {
            // Cancellation check per Surah
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var verse in surah.Verses)
            {
                // Cancellation check per Verse during string evaluation
                cancellationToken.ThrowIfCancellationRequested();

                string rawCombinedText = $"{verse.Translation} {verse.Transliteration} {verse.Text}";
                string normalizedCombinedText = StripPunctuation(rawCombinedText);

                // Exact phrase match ignoring punctuation & case -> 100% match (1.0)
                if (normalizedCombinedText.Contains(normalizedQueryPhrase, StringComparison.OrdinalIgnoreCase))
                {
                    double exactScore = 1.0;
                    flatVerseResults.Add((surah, CreateVerseResult(verse, exactScore)));
                    continue;
                }

                // Extract words from the target verse for word-by-word comparison
                var verseWords = Regex.Split(rawCombinedText, @"\W+")
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .ToList();

                double verseTotalScore = 0;
                bool hasMatch = false;

                foreach (var term in searchTerms)
                {
                    double maxTermScore = 0;
                    int exactMatches = 0;

                    foreach (var word in verseWords)
                    {
                        double similarity = GetSimilarityRatio(term, word);

                        if (string.Equals(term, word, StringComparison.OrdinalIgnoreCase))
                        {
                            exactMatches++;
                        }

                        if (similarity > maxTermScore)
                        {
                            maxTermScore = similarity;
                        }
                    }

                    if (maxTermScore >= 0.75)
                    {
                        hasMatch = true;

                        if (exactMatches > 1)
                        {
                            maxTermScore += 0.25 * (exactMatches - 1);
                        }

                        verseTotalScore += maxTermScore;
                    }
                }

                if (hasMatch)
                {
                    // Calculate individual verse score
                    double baseScore = verseTotalScore / searchTerms.Count;
                    double finalVerseScore = Math.Min(0.99, Math.Round(baseScore, 4));

                    flatVerseResults.Add((surah, CreateVerseResult(verse, finalVerseScore)));
                }
            }
        }

        // 4. Sort globally by verse score, apply topK limit to total VERSES, then group back by Surah
        var topVerses = flatVerseResults
            .Where(q => q.VerseResult.SimilarityScore >= 0.50)
            .OrderByDescending(item => item.VerseResult.SimilarityScore);

        IEnumerable<(Surah Surah, VerseResult VerseResult)> limitedVerses = topK.HasValue
            ? topVerses.Take(topK.Value)
            : topVerses;

        var groupedResults = limitedVerses
            .GroupBy(item => item.Surah.Id)
            .Select(group =>
            {
                var surahInfo = group.First().Surah;
                var verses = group.Select(item => item.VerseResult).ToList();
                double? maxScore = verses.Max(v => v.SimilarityScore);

                return new SurahResult
                {
                    Id = surahInfo.Id,
                    Name = surahInfo.Name,
                    Translation = surahInfo.Translation,
                    Transliteration = surahInfo.Transliteration,
                    Type = surahInfo.Type,
                    TotalVerses = surahInfo.TotalVerses,
                    VerseResults = verses,
                    SimilarityScore = maxScore
                };
            })
            .OrderByDescending(r => r.SimilarityScore)
            .ThenBy(r => r.Id)
            .ToList();

        return Task.FromResult(groupedResults);
    }

    private static VerseResult CreateVerseResult(Verse verse, double score)
    {
        return new VerseResult
        {
            Id = verse.Id,
            Text = verse.Text,
            Translation = verse.Translation,
            Transliteration = verse.Transliteration,
            SimilarityScore = score
        };
    }

    private static string StripPunctuation(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Remove common punctuation marks while normalizing multiple spaces into single spaces
        var clean = Regex.Replace(text, @"[^\w\s]", " ");
        return Regex.Replace(clean, @"\s+", " ").Trim();
    }

    private static double GetSimilarityRatio(string source, string target)
    {
        if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        // Substring Match (e.g. "gon" inside "dragon")
        if (target.Contains(source, StringComparison.OrdinalIgnoreCase))
        {
            double subRatio = (double)source.Length / target.Length;
            return subRatio >= 0.5 ? subRatio : 0.0;
        }

        // Require terms to be at least 5 letters for fuzzy edit-distance matching
        if (source.Length < 5) return 0.0;

        int distance = LevenshteinDistance(source.ToLowerInvariant(), target.ToLowerInvariant());
        int maxLength = Math.Max(source.Length, target.Length);

        if (maxLength == 0) return 1.0;

        double similarity = 1.0 - ((double)distance / maxLength);

        // Strict cutoff for fuzzy non-substring matches to prevent false positives like "jasus" ~ "sayran"
        return similarity >= 0.75 ? similarity : 0.0;
    }

    private static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
}