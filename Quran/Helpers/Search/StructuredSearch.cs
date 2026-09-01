using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search;

public class StructuredSearch : ISearch
{
    public bool GetSearchMode(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return false;

        return searchText.StartsWith(">", StringComparison.OrdinalIgnoreCase);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task<List<SurahResult>> PerformSearch(string searchText, CancellationToken cancellationToken = default)
    {
        // 1. Initial cancellation check
        cancellationToken.ThrowIfCancellationRequested();

        var list = new List<SurahResult>();

        if (string.IsNullOrWhiteSpace(searchText))
            return Task.FromResult(list);

        // Remove the leading '>' character and trim surrounding whitespace
        string cleanSearchText = searchText.Trim();
        if (cleanSearchText.StartsWith(">"))
        {
            cleanSearchText = cleanSearchText[1..].Trim();
        }

        if (string.IsNullOrWhiteSpace(cleanSearchText))
            return Task.FromResult(list);

        var searches = cleanSearchText.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        var regex = new Regex(
            @"^(?<surah>.+?)(?::(?<startVerse>\d+)?(?:-(?<endVerse>\d+))?)?$",
            RegexOptions.IgnoreCase);

        foreach (var search in searches)
        {
            // 2. Cancellation check inside the loop for multi-part queries
            cancellationToken.ThrowIfCancellationRequested();

            var match = regex.Match(search);

            if (!match.Success)
                continue;

            var surahSearch = match.Groups["surah"]
                .Value
                .Trim();

            int? startVerse = match.Groups["startVerse"].Success
                ? int.Parse(match.Groups["startVerse"].Value)
                : null;

            int? endVerse = match.Groups["endVerse"].Success
                ? int.Parse(match.Groups["endVerse"].Value)
                : null;

            // =====================================
            // Find Surah by ID or Name
            // =====================================

            SurahResult? surah;
            Surah? baseSurah = null;

            if (int.TryParse(surahSearch, out var surahId))
            {
                baseSurah = DataManager.Surahs.FirstOrDefault(s => s.Id == surahId);
            }
            else
            {
                baseSurah = DataManager.Surahs
                    .FirstOrDefault(s =>
                        s.Name.Contains(
                            surahSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        s.Transliteration.Contains(
                            surahSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        s.Translation.Contains(
                            surahSearch,
                            StringComparison.OrdinalIgnoreCase));
                
            }

            if (baseSurah is null)
                continue;

            surah = new SurahResult
            {
                Id = baseSurah.Id,
                Name = baseSurah.Name,
                Translation = baseSurah.Translation,
                Transliteration = baseSurah.Transliteration,
                Type = baseSurah.Type,
                TotalVerses = baseSurah.TotalVerses,
                VerseResults = baseSurah.Verses?.Select(v => new VerseResult
                {
                    Id = v.Id,
                    Text = v.Text,
                    Translation = v.Translation,
                    Transliteration = v.Transliteration,
                    SimilarityScore = 1.0
                }).ToList() ?? new List<VerseResult>(),
                SimilarityScore = 1.0
            };

            // =====================================
            // Entire Surah
            // Example: >Fatihah
            // =====================================

            if (!startVerse.HasValue)
            {
                list.Add(surah);
                continue;
            }

            // =====================================
            // Normalize verse range
            // =====================================

            var start = startVerse.Value;
            var end = endVerse ?? start;

            var min = Math.Min(start, end);
            var max = Math.Max(start, end);

            // =====================================
            // Find verses
            // =====================================

            var verses = surah.VerseResults
                .Where(v =>
                    v.Id >= min &&
                    v.Id <= max)
                .ToList();

            if (!verses.Any())
                continue;

            // =====================================
            // Add filtered Surah
            // =====================================

            list.Add(new SurahResult
            {
                Id = surah.Id,
                Name = surah.Name,
                Translation = surah.Translation,
                Transliteration = surah.Transliteration,
                SimilarityScore = 1.0,
                VerseResults = verses
            });
        }

        return Task.FromResult(list);
    }
}