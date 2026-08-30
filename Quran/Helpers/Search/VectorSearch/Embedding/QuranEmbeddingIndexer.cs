using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Model;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch.Embedding;

public class QuranEmbeddingIndexer(IEmbeddingService embeddingService)
{
    public async Task<List<VerseEmbedding>> CreateIndexAsync(
        IEnumerable<Surah> surahs,
        CancellationToken cancellationToken = default)
    {
        var embeddings = new List<VerseEmbedding>();

        foreach (var surah in surahs)
        foreach (var verse in surah.Verses)
        {
            var text =
                EmbeddingTextBuilder.Build(
                    surah,
                    verse);

            var vector =
                await embeddingService
                    .CreateEmbeddingAsync(
                        text,
                        cancellationToken);

            embeddings.Add(new VerseEmbedding
            {
                SurahId = surah.Id,
                VerseId = verse.Id,
                Vector = vector
            });
        }

        return embeddings;
    }
}