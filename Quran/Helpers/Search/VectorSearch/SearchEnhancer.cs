using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Quran.Helpers.Search.VectorSearch.Model;

namespace Quran.Helpers.Search.VectorSearch;

public class SearchEnhancer(List<SemanticSearchResult> searchResult, string query)
{
    private readonly List<SemanticSearchResult> _searchResult = searchResult;
    private readonly string _query = query;

    public List<SemanticSearchResult> EnhanceSearchResults()
    {
        try
        {
            var files = DataManager.GetFile("*.json");
            var matchedFiles = files.Where(f => f.Name.ToLower().Split("_").Any(e => _query.Contains(e))).ToList();
            List<SemanticSearchResult> data = new List<SemanticSearchResult>();
            foreach (var matchedFile in matchedFiles)
            {
                var jsonFile = DataManager.GetData<SemanticSearchResult>(matchedFile.Name);
                data = data.UnionBy(jsonFile, x => (x.SurahId, x.VerseId)).ToList();
            }

            if (!data.Any())
            {
                return _searchResult;
            }

            data.ForEach(q => q.Bookmarked = true);
            // Force static standard LINQ execution to bypass System.LinqAsync ambiguity
            var combinedData = data.UnionBy(
                _searchResult,
                x => (x.SurahId, x.VerseId)
            ).ToList();


            return combinedData;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error merging search results: {e.Message}");
            return _searchResult;
        }
    }
}