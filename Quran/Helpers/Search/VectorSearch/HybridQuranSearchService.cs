using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Embedding;
using Quran.Helpers.Search.VectorSearch.Model;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch;

public class HybridQuranSearchService : ASemanticSearchService
{
    private readonly SimpleBm25Scorer _bm25Scorer;

    public HybridQuranSearchService(IEmbeddingService embeddingService,
        List<VerseEmbedding> embeddings,
        List<SurahResult> surahs) : base(embeddingService, embeddings)
    {
        _bm25Scorer = new SimpleBm25Scorer(surahs);
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in",
        "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the",
        "their", "then", "there", "these", "they", "this", "to", "was", "will", "with"
    };

    private static readonly Dictionary<string, string[]> EntityAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "jesus", new[] { "jesus", "isa", "عيسى", "maseeh", "messiah" } },
        { "mary", new[] { "mary", "maryam", "مريم" } },
        { "moses", new[] { "moses", "musa", "موسى" } },
        { "pharaoh", new[] { "pharaoh", "firawn", "فرعون" } },
        { "abraham", new[] { "abraham", "ibrahim", "إبراهيم" } },
        { "joseph", new[] { "joseph", "yusuf", "يوسف" } }
    };

    public override async Task<List<SemanticSearchResult>> SearchAsync(List<SurahResult> surahs,
        string rawQuery,
        int maxResults = 100,
        bool fastSearch = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 1. Get Vector Scores
        var queryText = EmbeddingTextBuilder.BuildQuery(rawQuery);
        var queryVector = await embeddingService.CreateEmbeddingAsync(queryText, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var vectorRankings = embeddings
            .Select(item => new
            {
                item.SurahId,
                item.VerseId,
                Score = (float)VectorMath.CosineSimilarity(queryVector, item.Vector)
            })
            .OrderByDescending(x => x.Score)
            .Select((x, rank) => new { x.SurahId, x.VerseId, x.Score, Rank = rank + 1 })
            .ToDictionary(x => SimpleBm25Scorer.GetKey(x.SurahId, x.VerseId));

        // 2. Get BM25 Scores
        var bm25Scores = _bm25Scorer.ScoreQuery(rawQuery);
        var bm25Rankings = bm25Scores
            .OrderByDescending(x => x.Value)
            .Select((x, rank) => new { DocId = x.Key, Score = x.Value, Rank = rank + 1 })
            .ToDictionary(x => x.DocId);

        // 3. Perform Reciprocal Rank Fusion (RRF)
        const int k = 60; // Standard RRF constant

        // Theoretical maximum RRF score for rank #1 in both models (1/61 + 1/61)
        const double maxPossibleRrfScore = 1.0 / (k + 1) * 2.0;

        var allDocIds = vectorRankings.Keys.Union(bm25Rankings.Keys);

        var fusedResults = new List<SemanticSearchResult>();

        foreach (var docId in allDocIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double rrfScore = 0;
            if (vectorRankings.TryGetValue(docId, out var vec)) rrfScore += 1.0 / (k + vec.Rank);

            if (bm25Rankings.TryGetValue(docId, out var bm25)) rrfScore += 1.0 / (k + bm25.Rank);

            var surahId = docId >> 16;
            var verseId = docId & 0xFFFF;

            // Normalize RRF score to 0.0 - 1.0 scale relative to maximum possible score
            var normalizedScore = Math.Min(1.0, rrfScore / maxPossibleRrfScore);

            fusedResults.Add(new SemanticSearchResult
            {
                SurahId = surahId,
                VerseId = verseId,
                Score = (float)normalizedScore
            });
        }

        var result = fusedResults
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .ToList();
        if (!fastSearch)
            foreach (var semanticSearchResult in result)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var verse = surahs.First(s => s.Id == semanticSearchResult.SurahId).VerseResults
                    .First(v => v.Id == semanticSearchResult.VerseId);

                semanticSearchResult.Impacts = await GetWordImpactByOcclusionAsync(rawQuery, verse, cancellationToken);
            }

        return result;
    }

    private async Task<List<WordMappingResult>> GetWordImpactByOcclusionAsync(
        string queryText,
        Verse verse,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rawTokens = verse.Translation.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (rawTokens.Length == 0) return new List<WordMappingResult>();

        // 1. Clean punctuation and filter out stop-words
        var targets = rawTokens
            .Select((token, index) => new
            {
                Index = index,
                Raw = token,
                // Add hyphens and dash characters to the trim list
                Clean = token.Trim('.', ',', ';', ':', '?', '!', '"', '[', ']', '(', ')', '{', '}', '-', '—', '–')
            })
            // Ensure clean token is at least 1 character long and not in StopWords
            .Where(x => !string.IsNullOrWhiteSpace(x.Clean) && x.Clean.Length > 1 && !StopWords.Contains(x.Clean))
            .ToList();

        if (targets.Count == 0) return new List<WordMappingResult>();

        // 2. Compute baseline similarity
        var baseQueryVector =
            await embeddingService.CreateEmbeddingAsync(EmbeddingTextBuilder.BuildQuery(queryText), cancellationToken);
        var fullVerseVector = await embeddingService.CreateEmbeddingAsync(verse.Translation, cancellationToken);
        var baseScore = (float)VectorMath.CosineSimilarity(baseQueryVector, fullVerseVector);

        cancellationToken.ThrowIfCancellationRequested();

        // 3. Batch occluded embeddings concurrently
        var occlusionTasks = targets.Select(target =>
        {
            var occludedText = string.Join(" ", rawTokens.Where((_, idx) => idx != target.Index));
            return embeddingService.CreateEmbeddingAsync(occludedText, cancellationToken);
        }).ToList();

        var occludedVectors = await Task.WhenAll(occlusionTasks);

        cancellationToken.ThrowIfCancellationRequested();

        // 4. Calculate score drops
        var rawImpacts = new List<WordMappingResult>();
        for (var i = 0; i < targets.Count; i++)
        {
            var occludedScore = (float)VectorMath.CosineSimilarity(baseQueryVector, occludedVectors[i]);
            var impact = baseScore - occludedScore;

            rawImpacts.Add(new WordMappingResult
            {
                QueryWord = null,
                VerseWord = targets[i].Clean,
                CorrelationScore = impact
            });
        }

        // 5. Aggregate duplicate word tokens (calculate mean impact)
        var aggregated = rawImpacts
            .GroupBy(x => x.VerseWord, StringComparer.OrdinalIgnoreCase)
            .Select(g => new WordMappingResult
            {
                QueryWord = null,
                VerseWord = g.Key,
                CorrelationScore = g.Average(x => x.CorrelationScore)
            })
            .Where(x => x.CorrelationScore > 0)
            .OrderByDescending(x => x.CorrelationScore)
            .ToList();

        if (aggregated.Count == 0) return aggregated;

        // 6. Dynamic cutoff: Keep top terms whose impact is at least 25% of the highest impact word
        var maxScore = aggregated.Max(x => x.CorrelationScore);
        var dynamicCutoff = Math.Max(0.0015f, maxScore * 0.25f);

        return aggregated.Where(x => x.CorrelationScore > dynamicCutoff).ToList();
    }
}