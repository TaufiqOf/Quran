using System.Text.Json.Serialization;

namespace Quran.Models;

public sealed class Translator
{
    [JsonPropertyName("id")] 
    public string Id { get; set; } = null!;
    [JsonPropertyName("language")]
    public string Language { get; set; } = null!;
    [JsonPropertyName("translator")]
    public string Name { get; set; } = null!;
}