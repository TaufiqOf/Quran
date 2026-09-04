using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Quran.Models;

namespace Quran.Helpers;

public static class AskAiManager
{
    private static readonly IChatClient _chatClient;

    // Use deterministic decoding to reduce creative variance and hallucinations.
    private static readonly ChatOptions LowTemperatureChatOptions = new()
    {
        Temperature = 1
    };

    public static Action? ReadyToAskAi;

    static AskAiManager()
    {
        var aiSettings = SettingService.LoadAiSettings();
        _chatClient = AiClientFactory.Create(aiSettings.Provider, aiSettings.Endpoint, aiSettings.Model);
        SearchManager.SearcherRegistered += SearcherRegistered;
    }

    public static string PromptTemplate =>
        @"You are a context-only question answering system.

        You MUST follow these rules:

        1. Answer the user's question using ONLY the CONTEXT below.
        2. The CONTEXT is the only source of information.
        3. Do NOT use prior knowledge or external knowledge.
        4. If the CONTEXT contains information that answers the question, answer using that information.
        5. Do NOT refuse to answer because the subject is a religious figure, historical figure, person, or any other topic.
        6. If the answer is not explicitly supported by the CONTEXT, respond with EXACTLY:

        I cannot answer this query based on the provided context.

        7. Do not mention policies, safety restrictions, inability to provide biographies, or external limitations.
        8. Keep the answer concise.
        9. The Context is The Holy Quran. Refer the surah and verse number, as provided in the context.

        CONTEXT START
        {0}
        CONTEXT END

        Answer the user's question using ONLY CONTEXT START through CONTEXT END.";

    public static bool IsReady => SearchManager.IsSearcherRegistered;

    private static void SearcherRegistered()
    {
        ReadyToAskAi?.Invoke();
    }

    public static async IAsyncEnumerable<string> AskStreaming(
        string query,
        MessageResult result,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
                $"({q.Id}){q.Transliteration}\n" +
                string.Join("\n", q.VerseResults.Select(r => $"({r.Id}){r.Translation}"))));

        var systemPrompt = string.Format(
            PromptTemplate,
            contextText);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, query)
        };
        result.Context = context;
        await foreach (var update in _chatClient.GetStreamingResponseAsync(
                           messages,
                           LowTemperatureChatOptions,
                           cancellationToken))
            if (!string.IsNullOrEmpty(update.Text))
                result.Message += update.Text;

        result.IsSuccess = true;
    }

    public static async Task<MessageResult> Ask(string query, CancellationToken cancellationToken = default)
    {
        var formattedQuery = $"@{query.Trim()}:50";
        var context = await SearchManager.PerformSearch(formattedQuery, cancellationToken);

        // Guard against empty search results before invoking LLM
        if (!context.Any())
            return new MessageResult
            {
                IsSuccess = false,
                Message = "I cannot answer this query based on the provided context.",
                Context = context
            };

        var contextText = string.Join("\n",
            context.Select(q =>
                $"Surah {q.Id} ({q.Transliteration})\n" +
                string.Join("\n", q.VerseResults.Select(r =>
                    $"Surah {q.Id}, verse {r.Id}: {r.Translation}"))));
        var systemPrompt = string.Format(PromptTemplate, contextText);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, query)
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

    public class MessageResult : INotifyPropertyChanged

    {
        private List<SurahResult> _context = new();
        private bool _isSuccess;
        private string _message = string.Empty;

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