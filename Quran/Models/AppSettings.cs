using System.Collections.Generic;
using Quran.Helpers.Search.AiSearch;

namespace Quran.Models;

public sealed class AiSettings
{
    public AiProvider Provider { get; set; } = AiProvider.Ollama;
    public string Model { get; set; } = "llama3.2";
    public string Endpoint { get; set; } = "http://localhost:11434";
}
public sealed class AppSettings
{
    public string Language { get; set; } = "en";
    public string ReaderMode { get; set; } = "Quranic";
    
    public AiSettings AiSettings { get; set; } = new AiSettings();

}

public sealed class ChatModelSettings
{
    public List<ChatMessageModel> ChatMessages { get; set; } = new List<ChatMessageModel>();
}