using Newtonsoft.Json;

namespace Quran.Models;

public class HadithRootObject
{
    [JsonProperty("metadata")] public Metadata Metadata { get; set; } = null!;

    [JsonProperty("hadiths")] public Hadith[] Hadiths { get; set; } = null!;

    [JsonProperty("chapter")] public Chapter Chapter { get; set; } = null!;
}

public class Metadata
{
    [JsonProperty("length")] public int Length { get; set; }
    [JsonProperty("arabic")] public Arabic Arabic { get; set; } = null!;
    [JsonProperty("english")] public EnglishMetadata English { get; set; } = null!;
}

public class Arabic
{
    [JsonProperty("title")] public string Title { get; set; } = null!;
    [JsonProperty("author")] public string Author { get; set; } = null!;
    [JsonProperty("introduction")] public string Introduction { get; set; } = null!;
}

public class EnglishMetadata
{
    [JsonProperty("title")] public string Title { get; set; } = null!;
    [JsonProperty("author")] public string Author { get; set; } = null!;
    [JsonProperty("introduction")] public string Introduction { get; set; } = null!;
}

public class Hadith
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("idInBook")] public int IdInBook { get; set; }
    [JsonProperty("chapterId")] public int ChapterId { get; set; }
    [JsonProperty("bookId")] public int BookId { get; set; }
    [JsonProperty("arabic")] public string Arabic { get; set; } = null!;
    [JsonProperty("english")] public EnglishHadith English { get; set; } = null!;
}

public class EnglishHadith
{
    [JsonProperty("narrator")] public string Narrator { get; set; } = null!;
    [JsonProperty("text")] public string Text { get; set; } = null!;
}

public class Chapter
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("bookId")] public int BookId { get; set; }
    [JsonProperty("arabic")] public string Arabic { get; set; } = null!;
    [JsonProperty("english")] public string English { get; set; } = null!;
}