using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Quran.Helpers.Search.AiSearch;

public enum AiProvider
{
    OpenAI,
    Ollama,
    LLamaSharp,
    CustomLocalApi
}

public static class AiClientFactory
{
    /// <summary>
    ///     Checks asynchronously if the configured AI provider endpoint is alive and responsive.
    /// </summary>
    public static async Task<bool> IsClientAvailableAsync(
        AiProvider provider,
        IChatClient client,
        string targetModel = "",
        CancellationToken cancellationToken = default)
    {
        if (client == null) return false;

        try
        {
            switch (provider)
            {
                case AiProvider.Ollama:
                    // If the client is directly an OllamaApiClient, use native server health checks
                    if (client is OllamaApiClient ollamaClient)
                    {
                        var isRunning = await ollamaClient.IsRunningAsync(cancellationToken);
                        if (!isRunning) return false;

                        if (!string.IsNullOrWhiteSpace(targetModel))
                        {
                            var models = await ollamaClient.ListLocalModelsAsync(cancellationToken);
                            return models.Any(m => m.Name.StartsWith(targetModel, StringComparison.OrdinalIgnoreCase));
                        }

                        return true;
                    }

                    break;

                case AiProvider.OpenAI:
                case AiProvider.LLamaSharp:
                case AiProvider.CustomLocalApi:
                    // Perform a minimal dry-run execution to check connectivity
                    var testMessages = new[] { new ChatMessage(ChatRole.User, "ping") };
                    var response = await client.GetResponseAsync(testMessages, null, cancellationToken);
                    return response != null;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static IChatClient Create(AiProvider provider, string endpointOrKey, string modelName = "")
    {
        return provider switch
        {
            AiProvider.OpenAI =>
                new ChatClient(
                    string.IsNullOrEmpty(modelName) ? "gpt-4o-mini" : modelName,
                    endpointOrKey
                ).AsIChatClient(),

            AiProvider.Ollama =>
                new OllamaApiClient(
                    new HttpClient
                    {
                        BaseAddress = new Uri(
                            string.IsNullOrEmpty(endpointOrKey)
                                ? "http://localhost:11434"
                                : endpointOrKey),
                        Timeout = TimeSpan.FromMinutes(10)
                    },
                    string.IsNullOrEmpty(modelName)
                        ? "qwen2.5:14b"
                        : modelName
                ),

            AiProvider.LLamaSharp or AiProvider.CustomLocalApi =>
                new CustomLocalChatClient(endpointOrKey, modelName),

            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
}