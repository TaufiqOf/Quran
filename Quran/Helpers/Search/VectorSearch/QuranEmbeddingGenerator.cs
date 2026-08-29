using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Quran.Helpers.Search.VectorSearch.Embedding;
using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch;

public static class QuranEmbeddingGenerator
{
    public static async Task GenerateAsync(List<Surah> surahs, string modelPath, string tokenizerPath, string embeddingDataPath)
    {

        var tokenizer =
            new LocalTokenizer(tokenizerPath);

        using var embeddingService =
            new LocalEmbeddingService(
                modelPath,
                tokenizer);

        var indexer =
            new QuranEmbeddingIndexer(
                embeddingService);

        var embeddings =
            await indexer.CreateIndexAsync(
                surahs);

       

        await EmbeddingStorage.SaveAsync(
            embeddingDataPath,
            embeddings);
    }

}