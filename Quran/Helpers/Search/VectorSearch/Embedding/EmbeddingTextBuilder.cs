using Quran.Models;

namespace Quran.Helpers.Search.VectorSearch.Embedding;

public static class EmbeddingTextBuilder
{
    /// <summary>
    /// Formats queries for searching against E5 models.
    /// MUST use the 'query: ' prefix.
    /// </summary>
    public static string BuildQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "query: ";

        return $"query: {query.Trim().ToLowerInvariant()}";
    }

    /// <summary>
    /// Formats verse passages for E5 indexing.
    /// MUST use the 'passage: ' prefix and keep content clean to maximize accuracy.
    /// </summary>
    public static string BuildPassage(Surah surah, Verse verse)
    {
        // Includes Surah context along with English, Transliteration, and Arabic text
        string content = $"{surah.Translation} - {verse.Translation} {verse.Transliteration} {verse.Text}";
        return $"passage: {content.Trim()}";
    }

    /// <summary>
    /// Formats verse data into a readable string for UI display or debugging (NOT for vectors).
    /// </summary>
    public static string BuildDisplayText(Surah surah, Verse verse)
    {
        return $"""
                Quran
                Surah: {surah.Name} ({surah.Translation})
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