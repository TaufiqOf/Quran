using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Quran.Helpers.Search.AiSearch;

namespace Quran.Helpers;

public static class AskAiManager
{
    private static IChatClient _chatClient;
    public static Action? ReadyToAskAi;

    public static string PromptTemplate =>
        @"You are a strict factual assistant. Your task is to answer the user's query using ONLY the information provided in the Context section below.
        CRITICAL RULES TO PREVENT HALLUCINATIONS:
            Zero External Knowledge: Rely exclusively on the facts explicitly stated in the context. Do not bring in outside knowledge, assumptions, extrapolation, or logical jumps beyond what is directly supported.
            Handling Unanswerable Queries: If the answer to the query cannot be derived entirely from the provided context, state clearly and concisely: ""I cannot answer this query based on the provided context."" Do not guess or attempt to partially fulfill the answer with outside assumptions.
            Strict Quotes and Citations: Support your answer using direct quotes or close paraphrases from the context. Do not modify facts, dates, numbers, or key details.
            No Speculation: If the context is ambiguous, state that the context lacks sufficient detail rather than offering potential explanations.
        Context:
        {0}";

    public static bool IsReady => SearchManager.IsSearcherRegistered;

    static AskAiManager()
    {
        _chatClient = AiClientFactory.Create(AiProvider.Ollama, "http://localhost:11434", "llama3.2");
        SearchManager.SearcherRegistered += SearcherRegistered;
    }

    private static void SearcherRegistered()
    {
        ReadyToAskAi?.Invoke();
    }

    public static async Task<string> Ask(string query, CancellationToken cancellationToken = default)
    {
        var context = await SearchManager.PerformSearch(query);
        
        // Guard against empty search results before invoking LLM
        if (context == null || !context.Any())
        {
            return "I cannot answer this query based on the provided context.";
        }

        var contextText = string.Join("\n", context.Select(q => $"{q.Translation}"));
        var systemPrompt = string.Format(PromptTemplate, contextText);
        
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, query)
        };

        // Execute LLM call using Microsoft.Extensions.AI
        var response = await _chatClient.GetResponseAsync(messages, null, cancellationToken);
        
        // Extract plain text response from the message contents
        return response.Text ?? string.Empty;
    }
}