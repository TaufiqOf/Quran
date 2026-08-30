using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quran.Helpers.Search;
using Quran.Helpers.Search.VectorSearch;
using Quran.Models;

namespace Quran.Helpers;

public static class SearchManager
{
    public static Action? SearcherRegistered;
    public static bool IsSearcherRegistered { get; private set; } = false;
    private static List<ISearch>? Searcher { get; set; }

    static SearchManager()
    {
        Searcher = new List<ISearch>
        {
            new StructuredSearch(),
        };
    }

    public static async void RegisterSearcher()
    {
        var searcher = new VectorSearch();
        await searcher.InitializeAsync();
        Searcher?.Add(searcher);
        IsSearcherRegistered = true;
        SearcherRegistered?.Invoke();
    }

    public static async Task<List<Surah>> PerformSearch(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<Surah>();

        var searcher = Searcher?.FirstOrDefault(s => s.GetSearchMode(searchText));
        if (searcher == null)
            searcher = new TextSearch();
        return searcher?.PerformSearch(searchText) ?? new List<Surah>();
    }
}