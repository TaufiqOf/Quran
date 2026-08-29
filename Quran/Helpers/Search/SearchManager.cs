using System.Collections.Generic;
using System.Linq;
using Quran.Models;

namespace Quran.Helpers.Search;

public static class SearchManager
{
    public static List<ISearch> Searcher { get; } = new List<ISearch>
    {
        new StructuredSearch(),
        new TextSearch()
    };
    public static List<Surah> PerformSearch(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<Surah>();

        var searcher = Searcher.FirstOrDefault(s => s.GetSearchMode(searchText));

        return searcher?.PerformSearch(searchText) ?? new List<Surah>();
    }
}