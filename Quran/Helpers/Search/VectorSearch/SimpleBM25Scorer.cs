using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch;

public class SimpleBm25Scorer
{
    private const float k1 = 1.2f;
    private const float b = 0.75f;

    private static readonly HashSet<string> StopWords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "and", "are", "as",
            "at", "be", "but", "by",
            "for", "if", "in", "into",
            "is", "it", "of", "on",
            "or", "the", "to", "was",
            "were", "will", "with",
            "does", "do", "can", "allowed"
        };

    private readonly double _avgDocLength;
    private readonly Dictionary<int, int> _docLengths = new();
    private readonly Dictionary<string, int> _documentFrequencies = new();

    private readonly Dictionary<int, Dictionary<string, int>> _termFrequencies = new();
    private readonly int _totalDocs;

    public SimpleBm25Scorer(List<SurahResult> surahs)
    {
        var totalLength = 0;

        foreach (var surah in surahs)
        foreach (var verse in surah.VerseResults)
        {
            var compositeId = GetKey(surah.Id, verse.Id);
            var tokens = Tokenize($"{verse.Translation} {verse.Transliteration}").Where(t => !StopWords.Contains(t))
                .ToArray();

            _docLengths[compositeId] = tokens.Length;
            totalLength += tokens.Length;

            var tfMap = tokens
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());

            _termFrequencies[compositeId] = tfMap;

            foreach (var term in tfMap.Keys)
                _documentFrequencies[term] = _documentFrequencies.GetValueOrDefault(term, 0) + 1;
        }

        _totalDocs = _docLengths.Count;

        _avgDocLength = _totalDocs > 0
            ? (double)totalLength / _totalDocs
            : 1.0;
    }

    public Dictionary<int, float> ScoreQuery(string query)
    {
        var queryTokens = Tokenize(query)
            .Where(t => !StopWords.Contains(t))
            .Distinct()
            .ToArray();

        var scores = new Dictionary<int, float>();

        foreach (var (docId, tfMap) in _termFrequencies)
        {
            float score = 0;
            var docLen = _docLengths[docId];

            foreach (var token in queryTokens)
            {
                if (!tfMap.TryGetValue(token, out var tf))
                    continue;

                var df =
                    _documentFrequencies.GetValueOrDefault(token);

                var idf = MathF.Log(
                    (_totalDocs - df + 0.5f) /
                    (df + 0.5f) + 1f);

                var denominator =
                    tf + k1 *
                    (1f - b +
                     b * (docLen / _avgDocLength));

                score +=
                    idf *
                    (tf * (k1 + 1f) / (float)denominator);
            }

            if (score > 0)
                scores[docId] = score;
        }

        return scores;
    }

    private static string[] Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        return Regex
            .Matches(text.ToLowerInvariant(), @"[\p{L}\p{M}]+")
            .Select(m => NormalizeToken(m.Value))
            .Where(t => t.Length > 1)
            .ToArray();
    }

    private static string NormalizeToken(string token)
    {
        if (token.EndsWith("ying") && token.Length > 5)
            return token[..^4] + "y";

        return token;
    }

    public static int GetKey(int surahId, int verseId)
    {
        return (surahId << 16) | verseId;
    }
}