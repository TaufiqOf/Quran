using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Quran.Helpers.Search.VectorSearch.Model;

namespace Quran.Helpers.Search.VectorSearch.Embedding;

public sealed class LocalEmbeddingService :
    IEmbeddingService,
    IDisposable
{
    private const int MaxLength = 512;
    private readonly InferenceSession _session;
    private readonly ITokenizer _tokenizer;

    public LocalEmbeddingService(
        string modelPath,
        ITokenizer tokenizer)
    {
        _tokenizer = tokenizer;
        _session = new InferenceSession(modelPath);
    }

    public void Dispose()
    {
        _session.Dispose();
        if (_tokenizer is IDisposable disposable) disposable.Dispose();
    }

    public Task<float[]> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokens = _tokenizer.Encode(text, MaxLength);

        var inputIds = new DenseTensor<long>(new[] { 1, tokens.InputIds.Length });
        var attentionMask = new DenseTensor<long>(new[] { 1, tokens.AttentionMask.Length });

        for (var i = 0; i < tokens.InputIds.Length; i++)
        {
            inputIds[0, i] = tokens.InputIds[i];
            attentionMask[0, i] = tokens.AttentionMask[i];
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
        };

        if (_session.InputMetadata.ContainsKey("token_type_ids"))
        {
            var tokenTypeIds = new DenseTensor<long>(new[] { 1, tokens.InputIds.Length });
            inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds));
        }

        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        var embedding = MeanPoolSingle(output, tokens.AttentionMask);
        Normalize(embedding);

        return Task.FromResult(embedding);
    }

    public Task<List<float[]>> CreateEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (texts == null || texts.Count == 0)
            return Task.FromResult(new List<float[]>());

        var batchSize = texts.Count;

        // 1. Tokenize all texts in the batch
        var encodedBatch = texts
            .Select(t => _tokenizer.Encode(t, MaxLength))
            .ToList();

        // 2. Find max sequence length across batch for proper padding
        var maxSeqLength = encodedBatch.Max(b => b.InputIds.Length);

        // 3. Allocate batch Tensors [BatchSize, MaxSeqLength]
        var inputIds = new DenseTensor<long>(new[] { batchSize, maxSeqLength });
        var attentionMask = new DenseTensor<long>(new[] { batchSize, maxSeqLength });

        for (var batchIdx = 0; batchIdx < batchSize; batchIdx++)
        {
            var tokens = encodedBatch[batchIdx];

            for (var seqIdx = 0; seqIdx < maxSeqLength; seqIdx++)
                if (seqIdx < tokens.InputIds.Length)
                {
                    inputIds[batchIdx, seqIdx] = tokens.InputIds[seqIdx];
                    attentionMask[batchIdx, seqIdx] = tokens.AttentionMask[seqIdx];
                }
                else
                {
                    // Pad with 0s for shorter sequences in the batch
                    inputIds[batchIdx, seqIdx] = 0;
                    attentionMask[batchIdx, seqIdx] = 0;
                }
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
        };

        if (_session.InputMetadata.ContainsKey("token_type_ids"))
        {
            var tokenTypeIds = new DenseTensor<long>(new[] { batchSize, maxSeqLength });
            inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds));
        }

        // 4. Single execution step for full batch
        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();

        // 5. Mean pool and normalize each item in the batch
        var embeddings = new List<float[]>(batchSize);
        for (var batchIdx = 0; batchIdx < batchSize; batchIdx++)
        {
            var embedding = MeanPoolBatchItem(
                output,
                encodedBatch[batchIdx].AttentionMask,
                batchIdx);

            Normalize(embedding);
            embeddings.Add(embedding);
        }

        return Task.FromResult(embeddings);
    }

    private static float[] MeanPoolSingle(
        Tensor<float> output,
        long[] attentionMask)
    {
        var sequenceLength = output.Dimensions[1];
        var hiddenSize = output.Dimensions[2];
        var embedding = new float[hiddenSize];
        double validTokenCount = 0;

        for (var token = 0; token < sequenceLength; token++)
        {
            if (attentionMask[token] == 0) continue;

            validTokenCount++;
            for (var dim = 0; dim < hiddenSize; dim++) embedding[dim] += output[0, token, dim];
        }

        if (validTokenCount > 0)
            for (var i = 0; i < embedding.Length; i++)
                embedding[i] /= (float)validTokenCount;

        return embedding;
    }

    private static float[] MeanPoolBatchItem(
        Tensor<float> output,
        long[] attentionMask,
        int batchIndex)
    {
        var hiddenSize = output.Dimensions[2];
        var embedding = new float[hiddenSize];
        double validTokenCount = 0;

        for (var token = 0; token < attentionMask.Length; token++)
        {
            if (attentionMask[token] == 0) continue;

            validTokenCount++;
            for (var dim = 0; dim < hiddenSize; dim++) embedding[dim] += output[batchIndex, token, dim];
        }

        if (validTokenCount > 0)
            for (var i = 0; i < embedding.Length; i++)
                embedding[i] /= (float)validTokenCount;

        return embedding;
    }

    private static void Normalize(float[] vector)
    {
        double sum = 0;
        foreach (var value in vector) sum += value * value;

        var magnitude = Math.Sqrt(sum);
        if (magnitude == 0) return;

        for (var i = 0; i < vector.Length; i++) vector[i] = (float)(vector[i] / magnitude);
    }
}