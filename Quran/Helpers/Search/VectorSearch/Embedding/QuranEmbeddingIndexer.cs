using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Model;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch.Embedding;

public class QuranEmbeddingIndexer(IEmbeddingService embeddingService)
{
    private const int BatchSize = 32;

    public async Task<List<VerseEmbedding>> CreateIndexAsync(
        IEnumerable<Surah> surahs,
        CancellationToken cancellationToken = default)
    {
        var targetVerses = surahs
            .SelectMany(s => s.Verses.Select(v => (Surah: s, Verse: v)))
            .ToList();

        var embeddings = new List<VerseEmbedding>(targetVerses.Count);

        foreach (var chunk in targetVerses.Chunk(BatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var texts = chunk
                .Select(item => EmbeddingTextBuilder.BuildPassage(item.Surah, item.Verse))
                .ToList();

            // Requires IEmbeddingService.CreateEmbeddingsAsync(IEnumerable<string>, CancellationToken)
            var vectors = await embeddingService
                .CreateEmbeddingsAsync(texts, cancellationToken);

            for (var i = 0; i < chunk.Length; i++)
                embeddings.Add(new VerseEmbedding
                {
                    SurahId = chunk[i].Surah.Id,
                    VerseId = chunk[i].Verse.Id,
                    Vector = vectors[i]
                });
        }

        return embeddings;
    }
}