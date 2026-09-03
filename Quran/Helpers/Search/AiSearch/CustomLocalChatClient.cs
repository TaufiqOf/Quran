using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Quran.Helpers.Search.AiSearch;

public class CustomLocalChatClient : IChatClient
{
    private readonly string _endpointOrKey;
    private readonly string _modelName;

    public CustomLocalChatClient(string endpointOrKey, string modelName)
    {
        _endpointOrKey = endpointOrKey;
        _modelName = modelName;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var userPrompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

        var resultText = await RunInferenceAsync(userPrompt, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, resultText));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var userPrompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;
        var resultText = await RunInferenceAsync(userPrompt, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Use constructor instead of property initializer
        yield return new ChatResponseUpdate(ChatRole.Assistant, resultText);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType == typeof(CustomLocalChatClient) ? this : null;
    }

    public void Dispose()
    {
    }

    private async Task<string> RunInferenceAsync(string prompt, CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return $"Processed: {prompt}";
    }
}