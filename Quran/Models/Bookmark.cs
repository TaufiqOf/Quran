using System;

namespace Quran.Models;

public class Bookmark
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int SurahId { get; set; }
    public int VerseId { get; set; }
    public DateTime Timestamp { get; set; }
}