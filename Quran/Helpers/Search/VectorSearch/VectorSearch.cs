using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Embedding;
using Quran.Helpers.Search.VectorSearch.Model;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch;

public class VectorSearch : ISearch
{
    private QuranSemanticSearchService _quranSemanticSearchService;

    private readonly List<Surah> _surahs;
    private readonly string _embeddingDataPath;
    private readonly string _modelPath;
    private readonly string _tokenizerPath;
    private IEmbeddingService _embeddingService;

    public VectorSearch()
    {

        _embeddingDataPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Storage",
                "embeddings.json");

        _modelPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Storage",
                "model1.onnx");
        _tokenizerPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Storage",
                "tokenizer.json");
        var tokenizer = new LocalTokenizer(_tokenizerPath);

        _surahs =
            DataManager.Surahs;
        
        _embeddingService =
            new LocalEmbeddingService(
                _modelPath,
                tokenizer);

    }
    
    public async Task InitializeAsync()
    {
        // if (!File.Exists(_embeddingDataPath))
        // {
        //     await QuranEmbeddingGenerator.GenerateAsync(
        //         _surahs,
        //         _modelPath,
        //         _tokenizerPath,
        //         _embeddingDataPath);
        // }

        var embeddings =
            await EmbeddingStorage.LoadAsync(
                _embeddingDataPath);

        _quranSemanticSearchService =
            new QuranSemanticSearchService(
                _embeddingService,
                embeddings);

    }
    public bool GetSearchMode(
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return false;
        }

        return searchText.StartsWith(
            "?",
            StringComparison.Ordinal);
    }

    public List<Surah> PerformSearch(
        string searchText)
    {
        var query = searchText.Trim();

        if (query.StartsWith("?"))
        {
            query = query[1..].Trim();
        }

        var results =
            _quranSemanticSearchService
                .SearchAsync(query)
                .GetAwaiter()
                .GetResult();

        return ConvertResultsToSurahs(results);
    }

    private List<Surah> ConvertResultsToSurahs(
        List<SemanticSearchResult> results)
    {
        var resultSurahs =
            new Dictionary<int, Surah>();

        foreach (var result in results)
        {
            var originalSurah =
                _surahs.FirstOrDefault(s => s.Id == result.SurahId);

            if (originalSurah == null)
            {
                continue;
            }

            var originalVerse =
                originalSurah.Verses.FirstOrDefault(v => v.Id == result.VerseId);

            if (originalVerse == null)
            {
                continue;
            }

            if (!resultSurahs.TryGetValue(
                    originalSurah.Id,
                    out var resultSurah))
            {
                resultSurah = new Surah
                {
                    Id = originalSurah.Id,
                    Name = originalSurah.Name,
                    Transliteration =
                        originalSurah.Transliteration,
                    Translation =
                        originalSurah.Translation,
                    Type = originalSurah.Type,
                    TotalVerses =
                        originalSurah.TotalVerses,
                    Verses = new List<Verse>()
                };

                resultSurahs.Add(
                    originalSurah.Id,
                    resultSurah);
            }

            resultSurah.Verses.Add(
                originalVerse);
        }

        return resultSurahs.Values
            .OrderBy(s => s.Id)
            .ToList();
    }
}