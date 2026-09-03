using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Quran.Helpers.Search.AiSearch;
using Quran.Models;

namespace Quran.Helpers;

public static class AskAiManager
{
    private static IChatClient _chatClient;
    // Use deterministic decoding to reduce creative variance and hallucinations.
    private static readonly ChatOptions LowTemperatureChatOptions = new()
    {
        Temperature = 0
    };
    public static Action? ReadyToAskAi;

    public static string PromptTemplate =>
        @"You are a strict factual assistant. Your task is to answer the user's query using ONLY the information provided in the Context section below, treating all statements within the context as absolute truth.

        CRITICAL RULES TO PREVENT HALLUCINATIONS:
        1. Zero External Knowledge & Absolute Ground Truth: Treat the information provided in the context as factually true. Rely exclusively on the facts explicitly stated in the context. Do not bring in outside knowledge, real-world facts, assumptions, extrapolation, or logical jumps beyond what is directly supported.
        2. Handling Unanswerable Queries: If the answer to the query cannot be derived entirely from the provided context, state clearly and concisely: ""I cannot answer this query based on the provided context."" Do not guess or attempt to partially fulfill the answer with outside assumptions.
        3. Strict Quotes and Citations: Support your answer using direct quotes or close paraphrases from the context. Do not modify facts, dates, numbers, or key details.
        4. No Speculation: If the context is ambiguous, state that the context lacks sufficient detail rather than offering potential explanations.

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
    public static async IAsyncEnumerable<string> AskStreaming(
        string query,
        MessageResult result,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var formattedQuery = $"@{query.Trim()}:50";
        var context = await SearchManager.PerformSearch(
            formattedQuery,
            cancellationToken);

        if (!context.Any())
        {
            yield return "I cannot answer this query based on the provided context.";
            yield break;
        }

        var contextText = string.Join("\n",
            context.Select(q =>
                string.Join("\n",
                    q.VerseResults.Select(r => r.Translation))));

        var systemPrompt = string.Format(
            PromptTemplate,
            contextText);

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, query)
        };
        result.Context = context;
        await foreach (var update in _chatClient.GetStreamingResponseAsync(
                           messages,
                           LowTemperatureChatOptions,
                           cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                result.Message += update.Text;
            }
        }
        result.IsSuccess = true;
    }
    public static async Task<MessageResult> Ask(string query, CancellationToken cancellationToken = default)
    {
        var formattedQuery = $"@{query.Trim()}:50";
        var context = await SearchManager.PerformSearch(formattedQuery, cancellationToken);

        // Guard against empty search results before invoking LLM
        if (!context.Any())
        {
            return new MessageResult
            {
                IsSuccess = false,
                Message = "I cannot answer this query based on the provided context.",
                Context = context
            };
        }

        var contextText = string.Join("\n",
            context.Select(q => string.Join("\n", q.VerseResults.Select(r => $"{r.Translation}"))));
        var systemPrompt = string.Format(PromptTemplate, contextText);

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, query)
        };

        // Execute LLM call using Microsoft.Extensions.AI
        var response = await _chatClient.GetResponseAsync(messages, LowTemperatureChatOptions, cancellationToken);

        // Extract plain text response from the message contents
        return new MessageResult
        {
            IsSuccess = true,
            Message = response.Text ?? string.Empty,
            Context = context
        };
    }

    public class MessageResult: INotifyPropertyChanged
    
    {
        private bool _isSuccess;
        private string _message = string.Empty;
        private List<SurahResult> _context = new List<SurahResult>();

        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetField(ref _isSuccess, value);
        }

        public string Message
        {
            get => _message;
            set => SetField(ref _message, value);
        }

        public List<SurahResult> Context
        {
            get => _context;
            set => SetField(ref _context, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}