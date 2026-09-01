using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Embedding;
using Quran.Helpers.Search.VectorSearch.Model;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch;

public class VectorSearch : ISearch
{
    private readonly string _embeddingDataPath;
    private readonly string _modelPath;
    private readonly string _tokenizerPath;
    private IEmbeddingService? _embeddingService;
    private ASemanticSearchService? _semanticSearchService;

    public VectorSearch()
    {
        _embeddingDataPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Storage",
                "embeddings.json");

        _modelPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Quran",
            "Storage",
            "model1.onnx");

        _tokenizerPath = Path.Combine(
            AppContext.BaseDirectory,
            "Storage",
            "tokenizer.json");
    }

    public async Task InitializeAsync()
    {
        if (!File.Exists(_modelPath))
            await DownloadHelper.DownloadFileAsync(
                "https://huggingface.co/intfloat/multilingual-e5-small/resolve/main/onnx/model.onnx?download=true",
                _modelPath);

        var tokenizer = new LocalTokenizer(_tokenizerPath);

        _embeddingService =
            new LocalEmbeddingService(
                _modelPath,
                tokenizer);

        if (!File.Exists(_embeddingDataPath))
        {
            await QuranEmbeddingGenerator.GenerateAsync(
                DataManager.Surahs,
                _modelPath,
                _tokenizerPath,
                _embeddingDataPath);
        }

        var embeddings =
            await EmbeddingStorage.LoadAsync(
                _embeddingDataPath);

        _semanticSearchService =
            new HybridQuranSearchService(
                _embeddingService,
                embeddings,
                DataManager.Surahs.Select(q => new SurahResult()
                {
                    Id = q.Id,
                    Name = q.Name,
                    Transliteration = q.Transliteration,
                    Translation = q.Translation,
                    Type = q.Type,
                    TotalVerses = q.TotalVerses,
                    VerseResults = q.Verses.Select(v => new VerseResult()
                    {
                        Id = v.Id,
                        Text = v.Text,
                        Transliteration = v.Transliteration,
                        Translation = v.Translation
                    }).ToList()
                }).ToList());
    }

    public bool GetSearchMode(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return false;

        return searchText.StartsWith("?", StringComparison.Ordinal);
    }

    public async Task<List<SurahResult>> PerformSearch(string searchText, CancellationToken cancellationToken = default)
    {
        // 1. Initial cancellation check
        cancellationToken.ThrowIfCancellationRequested();
        var fastSearch = false;
        var surahs = DataManager.Surahs.Select(q => new SurahResult
        {
            Id = q.Id,
            Name = q.Name,
            Transliteration = q.Transliteration,
            Translation = q.Translation,
            Type = q.Type,
            TotalVerses = q.TotalVerses,
            VerseResults = q.Verses.Select(v => new VerseResult
            {
                Id = v.Id,
                Text = v.Text,
                Transliteration = v.Transliteration,
                Translation = v.Translation
            }).ToList()
        }).ToList();

        if (_semanticSearchService == null)
            throw new InvalidOperationException("Search service is not initialized.");

        var query = searchText.Trim();

        if (query.StartsWith("?"))
            query = query[1..].Trim();

        // Default top-k result count
        int topK = 10;

        // Extract :N limit modifier if present
        var match = Regex.Match(query, @":(-?\d+)$");
        if (match.Success)
        {
            topK = int.Parse(match.Groups[1].Value);
            // Strip the :N part so the embedding model gets clean text ("heaven")
            query = query[..match.Index].Trim();
        }
        
        if (topK == -1)
        {
            topK = 50;
            fastSearch = true;
        }

        if (topK <= 0 || topK > 100)
            topK = 100;


        if (string.IsNullOrWhiteSpace(query))
            return new List<SurahResult>();

        // 2. Pass cancellation token down to the semantic vector search service
        var results = await _semanticSearchService.SearchAsync(surahs, query, topK,fastSearch, cancellationToken);

        return ConvertResultsToSurahs(surahs, results, topK, cancellationToken);
    }

    private List<SurahResult> ConvertResultsToSurahs(
        List<SurahResult> surahs,
        List<SemanticSearchResult> results,
        int topK,
        CancellationToken cancellationToken)
    {
        var resultSurahs = new Dictionary<int, SurahResult>();

        foreach (var result in results)
        {
            // 3. Check cancellation token during conversion
            cancellationToken.ThrowIfCancellationRequested();

            var originalSurah = surahs.FirstOrDefault(s => s.Id == result.SurahId);
            if (originalSurah == null) continue;

            var originalVerse = originalSurah.VerseResults.FirstOrDefault(v => v.Id == result.VerseId);
            if (originalVerse == null) continue;

            if (!resultSurahs.TryGetValue(originalSurah.Id, out var resultSurah))
            {
                resultSurah = new SurahResult
                {
                    Id = originalSurah.Id,
                    Name = originalSurah.Name,
                    Transliteration = originalSurah.Transliteration,
                    Translation = originalSurah.Translation,
                    Type = originalSurah.Type,
                    TotalVerses = originalSurah.TotalVerses,
                    VerseResults = new List<VerseResult>()
                };
                resultSurahs.Add(originalSurah.Id, resultSurah);
            }

            // Create specific VerseResult with the vector score
            var resultVerse = new VerseResult
            {
                Id = originalVerse.Id,
                Text = originalVerse.Text,
                Transliteration = originalVerse.Transliteration,
                Translation = originalVerse.Translation,
                SimilarityScore = result.Score,
                Impacts = result.Impacts
            };
            resultSurah.VerseResults.Add(resultVerse);

            // Set/Update the max score on the parent SurahResult
            if (!resultSurah.SimilarityScore.HasValue || resultVerse.SimilarityScore > resultSurah.SimilarityScore)
            {
                resultSurah.SimilarityScore = resultVerse.SimilarityScore;
            }
        }

        // Sort Surahs by highest similarity score, then limit by topK
        return resultSurahs.Values
            .OrderByDescending(s => s.SimilarityScore ?? 0)
            .ThenBy(s => s.Id)
            .Take(topK)
            .ToList();
    }
}