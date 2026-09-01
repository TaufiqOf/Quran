using System;
using Microsoft.Extensions.AI;
using OllamaSharp;

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
    public static IChatClient Create(AiProvider provider, string endpointOrKey, string modelName = "")
    {
        return provider switch
        {
            AiProvider.OpenAI => 
                new OpenAI.Chat.ChatClient(
                    string.IsNullOrEmpty(modelName) ? "gpt-4o-mini" : modelName, 
                    endpointOrKey
                ).AsIChatClient(),

            AiProvider.Ollama => 
                new OllamaApiClient(
                    new Uri(string.IsNullOrEmpty(endpointOrKey) ? "http://localhost:11434" : endpointOrKey), 
                    string.IsNullOrEmpty(modelName) ? "llama3.2" : modelName
                ),

            AiProvider.LLamaSharp or AiProvider.CustomLocalApi => 
                new CustomLocalChatClient(endpointOrKey, modelName),

            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
}