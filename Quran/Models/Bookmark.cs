using System;
using System.Collections.Generic;

namespace Quran.Models;

public class BookmarkHeader
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
public class Bookmark
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public int SurahId { get; set; }
    public int VerseId { get; set; }
    public DateTime Timestamp { get; set; }
}