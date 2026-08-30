using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quran.Helpers.Search;
using Quran.Helpers.Search.VectorSearch;
using Quran.Models;

namespace Quran.Helpers;

public static class SearchManager
{
    private static List<ISearch>? Searcher { get; set; }

    public static void RegisterSearcher()
    {
        Searcher = new List<ISearch>
        {
            new StructuredSearch(),
            new VectorSearch()
        };
    }

    public static async Task<List<Surah>> PerformSearch(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<Surah>();

        var searcher = Searcher?.FirstOrDefault(s => s.GetSearchMode(searchText));
        if (searcher == null)
            searcher = new TextSearch();
        await searcher.InitializeAsync();
        return searcher?.PerformSearch(searchText) ?? new List<Surah>();
    }
}