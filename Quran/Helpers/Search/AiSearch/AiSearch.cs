using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        _chatClient = chatClient;
    }

    public bool GetSearchMode(string searchText) => searchText.StartsWith("@", StringComparison.OrdinalIgnoreCase);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task<List<SurahResult>> PerformSearch(string searchText, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(searchText))
            return new List<SurahResult>();

        // 1. Parse query and extract limit (e.g., "@query:10" -> cleanQuery = "query", limit = 10)
        var (cleanQuery, limit) = ParseQueryAndLimit(searchText);
        if (string.IsNullOrWhiteSpace(cleanQuery))
            return new List<SurahResult>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(1500)); // 15-second safety timeout

        var fallbackFormattedQuery = $"{cleanQuery}:-1";

        try
        {
            var options = new ChatOptions
            {
                Tools = new List<AITool>
                {
                    AIFunctionFactory.Create(
                        _tools.FindExactWordMatchesAsync, 
                        name: nameof(_tools.FindExactWordMatchesAsync)
                    ),
                    AIFunctionFactory.Create(
                        _tools.FindTopicOrNarrativeAsync, 
                        name: nameof(_tools.FindTopicOrNarrativeAsync)
                    )
                }
            };

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, 
        @"You are a strict Quranic search tool router.
        Select the correct search tool for the query.
        When calling FindExactWordMatchesAsync, populate parameter 'term'.
        When calling FindTopicOrNarrativeAsync, populate parameter 'topic'.
        Do NOT reply with conversational text."),
                new ChatMessage(ChatRole.User, cleanQuery)
            };

            // Step 1: Execute single LLM routing call
            var response = await _chatClient.GetResponseAsync(messages, options, cts.Token);

            // Step 2: Extract requested tool call
            var functionCall = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionCallContent>()
                .FirstOrDefault();

            if (functionCall != null)
            {
                List<VerseReference>? references = null;

                if (functionCall.Name == nameof(_tools.FindExactWordMatchesAsync))
                {
                    string term = GetArgOrDefault(functionCall.Arguments, "term", cleanQuery);
                    references = await _tools.FindExactWordMatchesAsync(term, cts.Token);
                }
                else if (functionCall.Name == nameof(_tools.FindTopicOrNarrativeAsync))
                {
                    string topic = GetArgOrDefault(functionCall.Arguments, "topic", cleanQuery);
                    references = await _tools.FindTopicOrNarrativeAsync(topic, cts.Token);
                }

                if (references != null && references.Count > 0)
                {
                    var surahResults = BuildSurahResultsFromReferences(references);
                    return ApplyVerseLimit(surahResults, limit);
                }
            }

            // Fallback if no tool requested or zero matches returned
            var fallbackResults = await _fallbackSearch.PerformSearch(fallbackFormattedQuery, cancellationToken);
            return ApplyVerseLimit(fallbackResults, limit);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine("[AiSearch] AI request timed out. Executing local search fallback.");
            var fallbackResults = await _fallbackSearch.PerformSearch(fallbackFormattedQuery, cancellationToken);
            return ApplyVerseLimit(fallbackResults, limit);
        }
        catch (OperationCanceledException)
        {
            // User requested cancellation or started a new search
            return new List<SurahResult>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AiSearch Exception]: {ex.Message}");
            var fallbackResults = await _fallbackSearch.PerformSearch(fallbackFormattedQuery, cancellationToken);
            return ApplyVerseLimit(fallbackResults, limit);
        }
    }

    /// <summary>
    /// Extracts argument values from FunctionCallContent dictionary across JSON providers.
    /// </summary>
    private static string GetArgOrDefault(IDictionary<string, object?>? args, string paramName, string fallback)
    {
        if (args == null || !args.TryGetValue(paramName, out var rawVal) || rawVal is null)
            return fallback;

        if (rawVal is string strVal && !string.IsNullOrWhiteSpace(strVal))
            return strVal;

        if (rawVal is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
        {
            var val = jsonElement.GetString();
            if (!string.IsNullOrWhiteSpace(val)) return val;
        }

        return fallback;
    }

    private static (string CleanQuery, int Limit) ParseQueryAndLimit(string rawText)
    {
        string query = rawText.TrimStart('@').Trim();
        int defaultLimit = 10;

        var match = Regex.Match(query, @":(\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int parsedLimit))
        {
            query = query.Substring(0, match.Index).Trim();
            return (query, parsedLimit);
        }

        return (query, defaultLimit);
    }

    private static List<SurahResult> ApplyVerseLimit(List<SurahResult> results, int maxVerses)
    {
        if (results == null || !results.Any() || maxVerses <= 0)
            return new List<SurahResult>();

        // 1. Flatten all verses with their parent Surah reference, then pick top 'maxVerses' by score
        var topVersesWithSurahs = results
            .SelectMany(surah => surah.VerseResults.Select(verse => new { Surah = surah, Verse = verse }))
            .OrderByDescending(x => x.Verse.SimilarityScore)
            .Take(maxVerses)
            .ToList();

        // 2. Regroup the selected top verses back into their respective Surahs
        var limitedSurahs = topVersesWithSurahs
            .GroupBy(x => x.Surah.Id)
            .Select(group =>
            {
                var originalSurah = group.First().Surah;

                return new SurahResult
                {
                    Id = originalSurah.Id,
                    Name = originalSurah.Name,
                    Translation = originalSurah.Translation,
                    Transliteration = originalSurah.Transliteration,
                    Type = originalSurah.Type,
                    TotalVerses = originalSurah.TotalVerses,
                    SimilarityScore = originalSurah.SimilarityScore,
                    // Order selected verses by their original position or score
                    VerseResults = group.Select(x => x.Verse)
                        .OrderByDescending(v => v.SimilarityScore)
                        .ToList()
                };
            })
            .OrderByDescending(s => s.SimilarityScore)
            .ToList();

        return limitedSurahs;
    }

    private static List<SurahResult> BuildSurahResultsFromReferences(List<VerseReference> references)
    {
        return references
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
    }
}