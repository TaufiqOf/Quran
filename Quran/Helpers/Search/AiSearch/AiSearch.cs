using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Quran.Models;

namespace Quran.Helpers.Search.AiSearch;

public class AiSearch : ISearch
{
    private readonly IChatClient _chatClient;
    private readonly QuranSearchTools _tools;
    private readonly ISearch _fallbackSearch;

    public AiSearch(IChatClient chatClient, ISearch exactSearch, ISearch semanticSearch)
    {
        _fallbackSearch = semanticSearch;
        _tools = new QuranSearchTools(exactSearch, semanticSearch);

        // Build pipeline with automatic async function invocation
        _chatClient = new ChatClientBuilder(chatClient)
            .UseFunctionInvocation()
            .Build();
    }

    public bool GetSearchMode(string searchText) => true;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task<List<SurahResult>> PerformSearch(string searchText, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(searchText))
            return new List<SurahResult>();

        try
        {
            // Explicitly use List<AITool> to fix the compiler type mismatch
            var options = new ChatOptions
            {
                Tools = new List<AITool>
                {
                    AIFunctionFactory.Create(_tools.FindExactWordMatchesAsync),
                    AIFunctionFactory.Create(_tools.FindTopicOrNarrativeAsync)
                }
            };

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, @"You are an intelligent Quran search intent router.
Your job is to parse natural language queries and call the appropriate search tool asynchronously.
- If the user asks where an entity, person, or prophet is mentioned BY NAME (e.g. 'Where is Muhammad referred to by name?'), call 'FindExactWordMatchesAsync'.
- If the user asks about an event, story, or concept (e.g. 'the birth of Jesus'), call 'FindTopicOrNarrativeAsync'."),
                new ChatMessage(ChatRole.User, searchText)
            };

            var response = await _chatClient.GetResponseAsync(messages, options, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return await ReconstructSurahResultsFromMatchesAsync(searchText, response, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AiSearch Exception]: {ex.Message}");
            return await _fallbackSearch.PerformSearch(searchText, cancellationToken);
        }
    }

private async Task<List<SurahResult>> ReconstructSurahResultsFromMatchesAsync(
    string originalQuery, 
    ChatResponse response, 
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    // 1. Look for Function/Tool call results inside the ChatResponse messages
    var toolCallResults = response.Messages
        .SelectMany(m => m.Contents)
        .OfType<FunctionResultContent>()
        .Select(f => f.Result)
        .OfType<List<VerseReference>>()
        .FirstOrDefault();

    // 2. If a tool successfully returned verse references, map them back to SurahResults
    if (toolCallResults != null && toolCallResults.Count > 0)
    {
        return BuildSurahResultsFromReferences(toolCallResults);
    }

    // 3. Fallback: If no tools were invoked, execute standard search
    return await _fallbackSearch.PerformSearch(originalQuery, cancellationToken);
}

private static List<SurahResult> BuildSurahResultsFromReferences(List<VerseReference> references)
{
    // Group references by Surah ID to construct full SurahResult objects
    var grouped = references
        .GroupBy(r => r.SurahId)
        .Select(group =>
        {
            var surahData = DataManager.Surahs.FirstOrDefault(s => s.Id == group.Key);
            if (surahData == null) return null;

            var matchingVerses = group
                .Select(refItem =>
                {
                    var verse = surahData.Verses.FirstOrDefault(v => v.Id == refItem.VerseId);
                    if (verse == null) return null;

                    return new VerseResult
                    {
                        Id = verse.Id,
                        Text = verse.Text,
                        Translation = verse.Translation,
                        Transliteration = verse.Transliteration,
                        SimilarityScore = refItem.Score
                    };
                })
                .Where(v => v != null)
                .Cast<VerseResult>()
                .ToList();

            return new SurahResult
            {
                Id = surahData.Id,
                Name = surahData.Name,
                Translation = surahData.Translation,
                Transliteration = surahData.Transliteration,
                Type = surahData.Type,
                TotalVerses = surahData.TotalVerses,
                VerseResults = matchingVerses,
                SimilarityScore = matchingVerses.Max(v => v.SimilarityScore)
            };
        })
        .Where(sr => sr != null)
        .Cast<SurahResult>()
        .OrderByDescending(sr => sr.SimilarityScore)
        .ThenBy(sr => sr.Id)
        .ToList();

    return grouped;
}
}