using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch.Embedding;

public static class EmbeddingTextBuilder
{
    public static string BuildPassage(
        Surah surah,
        Verse verse)
    {
        return
            $"passage: {verse.Translation}";
    }

    public static string BuildQuery(
        string query)
    {
        return $"query: {query}";
    }
    public static string Build(
        Surah surah,
        Verse verse)
    {
        return $"""
                Quran
                Surah: {surah.Name}
                Surah Number: {surah.Id}
                Verse Number: {verse.Id}

                Arabic:
                {verse.Text}

                Translation:
                {verse.Translation}

                Transliteration:
                {verse.Transliteration}
                """;
    }
}