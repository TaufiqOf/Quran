using System.Text.Json.Serialization;

namespace Quran.Models;

public class SurahOrder
{
    [JsonPropertyName("surah")]
    public int SurahId { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }
}