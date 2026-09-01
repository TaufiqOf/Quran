using System.Linq;
using System.Threading.Tasks;

namespace Quran.Helpers;

public static class AskAiManager
{
    public static string PromptTemplate =>
        @"You are a strict factual assistant. Your task is to answer the user's query using ONLY the information provided in the Context section below.
        CRITICAL RULES TO PREVENT HALLUCINATIONS:
            Zero External Knowledge: Rely exclusively on the facts explicitly stated in the context. Do not bring in outside knowledge, assumptions, extrapolation, or logical jumps beyond what is directly supported.
            Handling Unanswerable Queries: If the answer to the query cannot be derived entirely from the provided context, state clearly and concisely: ""I cannot answer this query based on the provided context."" Do not guess or attempt to partially fulfill the answer with outside assumptions.
            Strict Quotes and Citations: Support your answer using direct quotes or close paraphrases from the context. Do not modify facts, dates, numbers, or key details.
            No Speculation: If the context is ambiguous, state that the context lacks sufficient detail rather than offering potential explanations.
        Context:
        {0}
        Query:
        {1}";

    public static bool IsReady => SearchManager.IsSearcherRegistered;

    static AskAiManager()
    {
    }

    public static async Task Ask(string query)
    {
        var context = await SearchManager.PerformSearch(query);
        var contextText = string.Join("\n", context.Select(q => $"{q.Translation}"));
        var prompt = string.Format(PromptTemplate, contextText, query);
        
    }
}