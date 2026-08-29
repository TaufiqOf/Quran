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
    private readonly InferenceSession _session;
    private readonly ITokenizer _tokenizer;

    private const int MaxLength = 512;

    public LocalEmbeddingService(
        string modelPath,
        ITokenizer tokenizer)
    {
        _tokenizer = tokenizer;

        _session = new InferenceSession(modelPath);
    }

    public Task<float[]> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokens = _tokenizer.Encode(
            text,
            MaxLength);

        var inputIds = new DenseTensor<long>(
            new[] { 1, tokens.InputIds.Length });

        var attentionMask = new DenseTensor<long>(
            new[] { 1, tokens.AttentionMask.Length });

        for (var i = 0; i < tokens.InputIds.Length; i++)
        {
            inputIds[0, i] = tokens.InputIds[i];

            attentionMask[0, i] =
                tokens.AttentionMask[i];
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(
                "input_ids",
                inputIds),

            NamedOnnxValue.CreateFromTensor(
                "attention_mask",
                attentionMask)
        };

        if (_session.InputMetadata.ContainsKey(
                "token_type_ids"))
        {
            var tokenTypeIds = new DenseTensor<long>(
                new[] { 1, tokens.InputIds.Length });

            inputs.Add(
                NamedOnnxValue.CreateFromTensor(
                    "token_type_ids",
                    tokenTypeIds));
        }

        using var results =
            _session.Run(inputs);

        var output = results
            .First()
            .AsTensor<float>();

        var embedding = MeanPool(
            output,
            tokens.AttentionMask);

        Normalize(embedding);

        return Task.FromResult(embedding);
    }

    private static float[] MeanPool(
        Tensor<float> output,
        long[] attentionMask)
    {
        var sequenceLength =
            output.Dimensions[1];

        var hiddenSize =
            output.Dimensions[2];

        var embedding =
            new float[hiddenSize];

        double validTokenCount = 0;

        for (var token = 0;
             token < sequenceLength;
             token++)
        {
            if (attentionMask[token] == 0)
            {
                continue;
            }

            validTokenCount++;

            for (var dimension = 0;
                 dimension < hiddenSize;
                 dimension++)
            {
                embedding[dimension] +=
                    output[0, token, dimension];
            }
        }

        if (validTokenCount > 0)
        {
            for (var i = 0;
                 i < embedding.Length;
                 i++)
            {
                embedding[i] /=
                    (float)validTokenCount;
            }
        }

        return embedding;
    }

    private static void Normalize(
        float[] vector)
    {
        double sum = 0;

        foreach (var value in vector)
        {
            sum += value * value;
        }

        var magnitude = Math.Sqrt(sum);

        if (magnitude == 0)
        {
            return;
        }

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] =
                (float)(vector[i] / magnitude);
        }
    }

    public void Dispose()
    {
        _session.Dispose();

        if (_tokenizer is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}