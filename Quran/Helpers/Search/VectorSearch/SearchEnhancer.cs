using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Quran.Helpers.Search.VectorSearch.Model;

namespace Quran.Helpers.Search.VectorSearch;

public class SearchEnhancer(List<SemanticSearchResult> searchResult, string query)
{
    private readonly List<SemanticSearchResult> _searchResult = searchResult ?? [];
    private readonly string _query = query?.Trim().ToLower() ?? string.Empty;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", 
        "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", 
        "by", "did", "do", "does", "doing", "down", "during", "each", "few", "for", "from", 
        "further", "had", "has", "have", "having", "he", "how", "i", "if", "in", "into", "is", 
        "it", "its", "just", "me", "more", "most", "my", "no", "nor", "not", "of", "off", "on", 
        "once", "only", "or", "other", "our", "out", "over", "own", "s", "same", "so", "some", 
        "such", "than", "that", "the", "their", "them", "then", "there", "these", "they", "this", 
        "those", "through", "to", "too", "under", "until", "up", "very", "was", "we", "were", 
        "what", "when", "where", "which", "while", "who", "whom", "why", "with", "you", "your"
    };

    public List<SemanticSearchResult> EnhanceSearchResults()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_query))
            {
                return _searchResult;
            }

            // 1. Extract, clean, and stem query tokens
            var queryTokens = ExtractAndStemTokens(_query);

            if (queryTokens.Count == 0)
            {
                return _searchResult;
            }

            var files = DataManager.GetFile("*.json");

            // 2. Filter files based on 60% stemmed token overlap
            var matchedFiles = files.Where(f =>
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(f.Name).ToLower();
                var fileTokens = ExtractAndStemTokens(fileNameWithoutExt);

                if (fileTokens.Count == 0) return false;

                int matchCount = fileTokens.Count(token => queryTokens.Contains(token));
                double matchPercentage = (double)matchCount / fileTokens.Count;

                if (queryTokens.Count > 1)
                {
                    return matchPercentage >= 0.60 && matchCount >= 2;
                }

                return matchPercentage >= 0.60;
            }).ToList();

            var pinnedResults = new List<SemanticSearchResult>();

            foreach (var matchedFile in matchedFiles)
            {
                var jsonFile = DataManager.GetData<SemanticSearchResult>(matchedFile.Name);
                if (jsonFile != null && jsonFile.Count > 0)
                {
                    pinnedResults.AddRange(jsonFile);
                }
            }

            if (pinnedResults.Count == 0)
            {
                return _searchResult;
            }

            // 3. Set top priority scores for pinned results
            foreach (var item in pinnedResults)
            {
                item.Bookmarked = true;
                item.Score = Math.Max(item.Score, 1.0);
            }

            // 4. Prefer pinned items over vector items
            var combinedDictionary = new Dictionary<(int SurahId, int VerseId), SemanticSearchResult>();

            foreach (var item in pinnedResults)
            {
                combinedDictionary[(item.SurahId, item.VerseId)] = item;
            }

            foreach (var item in _searchResult)
            {
                var key = (item.SurahId, item.VerseId);
                if (!combinedDictionary.ContainsKey(key))
                {
                    combinedDictionary[key] = item;
                }
            }

            return combinedDictionary.Values
                .OrderByDescending(x => x.Bookmarked)
                .ThenByDescending(x => x.Score)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error merging search results: {e.Message}");
            return _searchResult;
        }
    }

    private static HashSet<string> ExtractAndStemTokens(string text)
    {
        var rawTokens = text
            .Split(new[] { ' ', '-', '_', ',', '.', ':', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLower())
            .Where(t => !StopWords.Contains(t) && t.Length > 1);

        var stemmedTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in rawTokens)
        {
            stemmedTokens.Add(PorterStemmer.Stem(token));
        }

        return stemmedTokens;
    }
}