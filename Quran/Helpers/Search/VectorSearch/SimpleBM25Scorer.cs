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
            var tokens = Tokenize($"{verse.Translation} {verse.Transliteration}");

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
        _avgDocLength = (double)totalLength / _totalDocs;
    }

    public Dictionary<int, float> ScoreQuery(string query)
    {
        var queryTokens = Tokenize(query);
        var scores = new Dictionary<int, float>();

        foreach (var (docId, tfMap) in _termFrequencies)
        {
            var score = 0f;
            var docLen = _docLengths[docId];

            foreach (var token in queryTokens)
            {
                if (!tfMap.TryGetValue(token, out var tf)) continue;

                var df = _documentFrequencies.GetValueOrDefault(token, 0);
                var idf = (float)Math.Log((_totalDocs - df + 0.5) / (df + 0.5) + 1.0);

                var numerator = tf * (k1 + 1);
                var denominator = Convert.ToSingle(tf + k1 * (1 - b + b * (docLen / _avgDocLength)));

                score += idf * (numerator / denominator);
            }

            if (score > 0) scores[docId] = score;
        }

        return scores;
    }

    private static string[] Tokenize(string text)
    {
        return Regex.Split(text.ToLowerInvariant(), @"\W+").Where(t => t.Length > 1).ToArray();
    }

    public static int GetKey(int surahId, int verseId)
    {
        return (surahId << 16) | verseId;
    }
}