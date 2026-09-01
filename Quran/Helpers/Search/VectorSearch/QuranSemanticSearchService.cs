using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Embedding;
using Quran.Helpers.Search.VectorSearch.Model;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch;

public class QuranSemanticSearchService(IEmbeddingService embeddingService, List<VerseEmbedding> embeddings)
    : ASemanticSearchService(embeddingService, embeddings)
{
    public override async Task<List<SemanticSearchResult>> SearchAsync(List<SurahResult> surahs, string rawQuery,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        // Apply the 'query: ' prefix ONCE here
        var queryText = EmbeddingTextBuilder.BuildQuery(rawQuery);

        var queryVector = await embeddingService
            .CreateEmbeddingAsync(queryText, cancellationToken);

        var results = embeddings
            .Select(embedding => new SemanticSearchResult
            {
                SurahId = embedding.SurahId,
                VerseId = embedding.VerseId,
                Score = VectorMath.CosineSimilarity(queryVector, embedding.Vector)
            })
            // E5 cosine similarity scores for relevant matches typically range between 0.68 - 0.78
            .Where(x => x.Score >= 0.82f)
            .OrderByDescending(x => x.Score)
            .Take(maxResults)
            .ToList();

        return results;
    }
}