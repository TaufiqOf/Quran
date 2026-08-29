
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quran.Models;

namespace Quran.Helpers.Search;

public class TextSearch : ISearch
{
    public bool GetSearchMode(string searchText)
    {
        return !Regex.IsMatch(
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

    public List<Surah> PerformSearch(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return new List<Surah>();


        var words = searchText
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        return DataManager.Surahs
            .Where(s =>
                words.Any(word =>
                    s.Name.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    s.Translation.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    s.Transliteration.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                    s.Verses.Any(v =>
                        v.Text.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        v.Translation.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        v.Transliteration.Contains(word, StringComparison.OrdinalIgnoreCase))))
            .Select(s => new Surah
            {
                Id = s.Id,
                Name = s.Name,
                Translation = s.Translation,
                Transliteration = s.Transliteration,

                Verses = s.Verses
                    .Where(v =>
                        words.Any(word =>
                            v.Text.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                            v.Translation.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                            v.Transliteration.Contains(word, StringComparison.OrdinalIgnoreCase)))
                    .ToList()
            })
            .ToList();
    }
}