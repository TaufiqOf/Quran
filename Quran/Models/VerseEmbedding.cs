using System;

namespace Quran.Models;

public class VerseEmbedding
{
    public int SurahId { get; set; }

    public int VerseId { get; set; }

    public float[] Vector { get; set; } = Array.Empty<float>();

    public string Reference => $"{SurahId}:{VerseId}";
}