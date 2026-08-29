using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch.Embedding;

public static class EmbeddingStorage
{
    public static async Task SaveAsync(
        string filePath,
        List<VerseEmbedding> embeddings)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        var json =
            JsonSerializer.Serialize(
                embeddings,
                options);

        await File.WriteAllTextAsync(
            filePath,
            json);
    }

    public static async Task<List<VerseEmbedding>> LoadAsync(
        string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new List<VerseEmbedding>();
        }

        var json =
            await File.ReadAllTextAsync(filePath);

        return JsonSerializer.Deserialize<
                   List<VerseEmbedding>>(json)
               ?? new List<VerseEmbedding>();
    }

}