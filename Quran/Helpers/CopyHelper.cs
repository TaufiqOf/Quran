using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Quran.Models;

namespace Quran.Helpers;

public static class CopyHelper
{
    public static async Task CopyClipboard(TopLevel topLevel, string text)
    {
        var clipboard = topLevel.Clipboard;
        if (clipboard != null)
            await clipboard.SetTextAsync(text);
    }
    public static string FormatText(Surah surah)
    {
        var surahStructurePreference = SettingService.LoadCopySurahStructurePreference();
        var text = surahStructurePreference
            .Replace("{SurahNumber}", surah.Id.ToString())
            .Replace("{SurahName}", surah.Name)
            .Replace("{SurahTransliteration}", surah.Transliteration)
            .Replace("{SurahTranslation}", surah.Translation)
            .Replace("/n", Environment.NewLine)
            .Replace("\\n", Environment.NewLine)
            .Replace("\n", Environment.NewLine);
        return text;
    }

    public static string FormatText(Verse verse)
    {
        var verseStructurePreference = SettingService.LoadCopyVerseStructurePreference();
        var text = verseStructurePreference
            .Replace("{VerseNumber}", verse.Id.ToString())
            .Replace("{VerseText}", verse.Text)
            .Replace("{VerseTransliteration}", verse.Transliteration)
            .Replace("{VerseTranslation}", verse.Translation)
            // Replaces both literal escape variants and escaped newlines with OS-specific newline
            .Replace("/n", Environment.NewLine)
            .Replace("\\n", Environment.NewLine)
            .Replace("\n", Environment.NewLine);
        return text;
    }

}