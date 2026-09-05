using System.Collections.Generic;

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

    public AiSettings AiSettings { get; set; } = new();
    public string? CopySurahStructure { get; set; } = @"({SurahNumber}){SurahTransliteration}-{SurahName}({SurahTranslation})";
    public string? CopyVerseStructure { get; set; } = @"({VerseNumber}) {VerseText} /n({VerseTranslation})/n {VerseTransliteration}";
}

public sealed class ChatModelSettings
{
    public List<ChatMessageModel> ChatMessages { get; set; } = new();
}