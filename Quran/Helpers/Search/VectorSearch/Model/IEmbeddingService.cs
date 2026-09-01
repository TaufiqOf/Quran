using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Quran.Helpers.Search.VectorSearch.Model;

public interface IEmbeddingService
{
    Task<float[]> CreateEmbeddingAsync(
        string text, 
        CancellationToken cancellationToken = default);

    Task<List<float[]>> CreateEmbeddingsAsync(
        List<string> texts, 
        CancellationToken cancellationToken = default);
}