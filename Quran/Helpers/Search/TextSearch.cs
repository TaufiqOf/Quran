using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search;

public class TextSearch : ISearch
{
    public bool GetSearchMode(string searchText)
    {
        return true;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public List<Surah> PerformSearch(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<Surah>();
        }

        var words = searchText
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        return DataManager.Surahs
            .Select(s => new Surah
            {
                Id = s.Id,
                Name = s.Name,
                Translation = s.Translation,
                Transliteration = s.Transliteration,
                Type = s.Type,
                TotalVerses = s.TotalVerses,

                Verses = s.Verses
                    .Where(v =>
                        words.All(word =>
                            v.Text.Contains(
                                word,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            v.Translation.Contains(
                                word,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            v.Transliteration.Contains(
                                word,
                                StringComparison.OrdinalIgnoreCase)))
                    .ToList()
            })
            .Where(s => s.Verses.Count > 0)
            .ToList();
    }

}