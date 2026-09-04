using System;
using System.IO;
using System.Linq;
using Quran.Helpers.Search.VectorSearch.Model;
using Tokenizers.HuggingFace.Tokenizer;

namespace Quran.Helpers.Search.VectorSearch.Embedding;

public sealed class LocalTokenizer : ITokenizer, IDisposable
{
    private readonly Tokenizer _tokenizer;

    public LocalTokenizer(string tokenizerPath)
    {
        if (!File.Exists(tokenizerPath))
            throw new FileNotFoundException(
                "Tokenizer file was not found.",
                tokenizerPath);

        _tokenizer = Tokenizer.FromFile(tokenizerPath);
    }

    public void Dispose()
    {
        _tokenizer.Dispose();
    }

    public TokenizedInput Encode(
        string text,
        int maxLength)
    {
        var encoding = _tokenizer
            .Encode(
                text,
                true,
                includeTypeIds: true,
                includeAttentionMask: true)
            .First();

        var inputIds = encoding.Ids
            .Take(maxLength)
            .Select(x => (long)x)
            .ToArray();

        var attentionMask = encoding.AttentionMask
            .Take(maxLength)
            .Select(x => (long)x)
            .ToArray();

        return new TokenizedInput
        {
            InputIds = inputIds,
            AttentionMask = attentionMask
        };
    }

    public long PadTokenId { get; set; }
}