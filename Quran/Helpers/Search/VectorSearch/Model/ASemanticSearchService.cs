using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch.Model;

public abstract class ASemanticSearchService
{
    protected readonly IEmbeddingService embeddingService;
    protected readonly List<VerseEmbedding> embeddings;

    protected ASemanticSearchService(IEmbeddingService embeddingService, List<VerseEmbedding> embeddings)
    {
        this.embeddingService = embeddingService;
        this.embeddings = embeddings;
    }

    public abstract Task<List<SemanticSearchResult>> SearchAsync(
        List<SurahResult> surahs,
        string rawQuery,
        int maxResults = 100,
        bool fastSearch = false,
        CancellationToken cancellationToken = default);
}