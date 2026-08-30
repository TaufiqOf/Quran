using System;

namespace Quran.Helpers.Search.VectorSearch.Model;

public static class VectorMath
{
    public static double CosineSimilarity(
        float[] vectorA,
        float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException(
                "Vectors must have the same dimensions.");

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (var i = 0; i < vectorA.Length; i++)
        {
            dotProduct +=
                vectorA[i] * vectorB[i];

            magnitudeA +=
                vectorA[i] * vectorA[i];

            magnitudeB +=
                vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 ||
            magnitudeB == 0)
            return 0;

        return dotProduct /
               (Math.Sqrt(magnitudeA) *
                Math.Sqrt(magnitudeB));
    }
}