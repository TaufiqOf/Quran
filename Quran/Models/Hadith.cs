using Newtonsoft.Json;

namespace Quran.Models;

public class HadithRootObject
{
    [JsonProperty("metadata")]
    public Metadata Metadata { get; set; }
    [JsonProperty("hadiths")]
    public Hadith[] Hadiths { get; set; }
    [JsonProperty("chapter")]
    public Chapter Chapter { get; set; }
}

public class Metadata
{
    [JsonProperty("length")]
    public int Length { get; set; }
    [JsonProperty("arabic")]
    public Arabic Arabic { get; set; }
    [JsonProperty("english")]
    public EnglishMetadata English { get; set; }
}

public class Arabic
{
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("author")]
    public string Author { get; set; }
    [JsonProperty("introduction")]
    public string Introduction { get; set; }
}

public class EnglishMetadata
{
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("author")]
    public string Author { get; set; }
    [JsonProperty("introduction")]
    public string Introduction { get; set; }
}

public class Hadith
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("idInBook")]
    public int IdInBook { get; set; }
    [JsonProperty("chapterId")]
    public int ChapterId { get; set; }
    [JsonProperty("bookId")]
    public int BookId { get; set; }
    [JsonProperty("arabic")]
    public string Arabic { get; set; }
    [JsonProperty("english")]
    public EnglishHadith English { get; set; }
}

public class EnglishHadith
{
    [JsonProperty("narrator")]
    public string Narrator { get; set; }
    [JsonProperty("text")]
    public string Text { get; set; }
}

public class Chapter
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("bookId")]
    public int BookId { get; set; }
    [JsonProperty("arabic")]
    public string Arabic { get; set; }
    [JsonProperty("english")]
    public string English { get; set; }
}

