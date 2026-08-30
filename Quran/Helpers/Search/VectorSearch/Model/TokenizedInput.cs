using System;

namespace Quran.Helpers.Search.VectorSearch.Model;

public sealed class TokenizedInput
{
    public long[] InputIds { get; init; }
        = Array.Empty<long>();

    public long[] AttentionMask { get; init; }
        = Array.Empty<long>();
}