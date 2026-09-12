using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch.Model;

public abstract class ASemanticSearchService
{
    protected readonly IEmbeddingService EmbeddingService;
    protected readonly List<VerseEmbedding> Embeddings;

    protected ASemanticSearchService(IEmbeddingService embeddingService, List<VerseEmbedding> embeddings)
    {
        this.EmbeddingService = embeddingService;
        this.Embeddings = embeddings;
    }

    public abstract Task<List<SemanticSearchResult>> SearchAsync(
        List<SurahResult> surahs,
        string rawQuery,
        int maxResults = 100,
        bool fastSearch = false,
        CancellationToken cancellationToken = default);
}