using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search;

public class StructuredSearch : ISearch
{
    public bool GetSearchMode(string searchText)
    {
        return Regex.IsMatch(
            searchText,
            @"^\s*
          [\p{L}\d]+
          (?:\s+[\p{L}\d]+)*
          (?:
              :
              (?:\d+(?:-\d+)?)?
          )
          (?:\s*,\s*
              [\p{L}\d]+
              (?:\s+[\p{L}\d]+)*
              (?:
                  :
                  (?:\d+(?:-\d+)?)?
              )
          )*
          \s*$",
            RegexOptions.IgnorePatternWhitespace);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public List<Surah> PerformSearch(string searchText)
    {
        var list = new List<Surah>();

        if (string.IsNullOrWhiteSpace(searchText))
            return list;

        var searches = searchText.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        var regex = new Regex(
            @"^(?<surah>.+?)(?::(?<startVerse>\d+)?(?:-(?<endVerse>\d+))?)?$",
            RegexOptions.IgnoreCase);

        foreach (var search in searches)
        {
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

            Surah? surah;

            if (int.TryParse(surahSearch, out var surahId))
                surah = DataManager.Surahs
                    .FirstOrDefault(s => s.Id == surahId);
            else
                surah = DataManager.Surahs
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

            if (surah is null)
                continue;

            // =====================================
            // Entire Surah
            // Example: Fatihah
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

            var verses = surah.Verses
                .Where(v =>
                    v.Id >= min &&
                    v.Id <= max)
                .ToList();

            if (!verses.Any())
                continue;

            // =====================================
            // Add filtered Surah
            // =====================================

            list.Add(new Surah
            {
                Id = surah.Id,
                Name = surah.Name,
                Translation = surah.Translation,
                Transliteration = surah.Transliteration,
                Verses = verses
            });
        }

        return list;
    }
}