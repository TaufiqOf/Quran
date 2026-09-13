using Newtonsoft.Json;

namespace Quran.Models;

public class HadithChapter
{
    [JsonProperty("id")] public decimal Id { get; set; }
    [JsonProperty("bookId")] public int BookId { get; set; }
    [JsonProperty("arabic")] public string Arabic { get; set; } = string.Empty;
    [JsonProperty("english")] public string English { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"({Id}){Arabic} - {English}";
    }
}