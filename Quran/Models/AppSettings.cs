using System.Collections.Generic;

namespace Quran.Models;

public sealed class AppSettings
{
    public string Language { get; set; } = "en";
    public string ReaderMode { get; set; } = "Quranic";
    
    
    
    public List<ChatMessageModel> ChatMessages { get; set; } = new List<ChatMessageModel>();
}