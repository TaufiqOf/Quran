using System;

namespace Quran.Models;

public class Bookmark
{
    public int Id { get; set; }
    public int SurahId { get; set; }
    public int VerseId { get; set; }
    public DateTime Timestamp { get; set; }
}