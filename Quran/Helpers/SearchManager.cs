using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quran.Helpers.Search;
using Quran.Helpers.Search.StructuredSearch;
using Quran.Helpers.Search.TextSearch;
using Quran.Helpers.Search.VectorSearch;
using Quran.Models;

namespace Quran.Helpers;

public static class SearchManager
{
    public static Action? SearcherRegistered;

    static SearchManager()
    {
        Searcher = new List<ISearch>
        {
            new StructuredSearch()
        };
    }

    public static bool IsSearcherRegistered { get; private set; }
    private static List<ISearch>? Searcher { get; }

    public static async Task RegisterSearcher()
    {
        var searcher = new VectorSearch();
        await searcher.InitializeAsync();
        Searcher?.Add(searcher);
        IsSearcherRegistered = true;
        SearcherRegistered?.Invoke();
    }

    public static async Task<List<SurahResult>> PerformSearch(string? searchText,CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<SurahResult>();
        searchText = searchText.Trim();
        var searcher = Searcher?.FirstOrDefault(s => s.GetSearchMode(searchText));
        if (searcher == null)
            searcher = new TextSearch();
        return await searcher.PerformSearch(searchText, cancellationToken);
    }
}