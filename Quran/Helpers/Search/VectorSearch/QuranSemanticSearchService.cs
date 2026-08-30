using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Embedding;
using Quran.Helpers.Search.VectorSearch.Model;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch;

public class QuranSemanticSearchService(
    IEmbeddingService embeddingService,
    List<VerseEmbedding> embeddings)
{
    public async Task<List<SemanticSearchResult>> SearchAsync(
        string query,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        var queryText =
            EmbeddingTextBuilder.BuildQuery(query);

        var queryVector =
            await embeddingService
                .CreateEmbeddingAsync(
                    queryText,
                    cancellationToken);

        var results = embeddings
            .Select(embedding =>
                new SemanticSearchResult
                {
                    SurahId = embedding.SurahId,
                    VerseId = embedding.VerseId,

                    Score =
                        VectorMath.CosineSimilarity(
                            queryVector,
                            embedding.Vector)
                })
            .Where(x => x.Score > .8) // Filter out results with non-positive scores
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .ToList();

        return results;
    }

}

