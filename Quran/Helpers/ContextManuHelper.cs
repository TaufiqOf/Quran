using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Quran.Models;

namespace Quran.Helpers;

public static class ContextMenuHelper
{
    public static async Task CopyTranslationRequested(
        TopLevel? topLevel,
        Verse verse)
    {
        var clipboard = topLevel?.Clipboard;

        if (clipboard != null)
            await clipboard.SetTextAsync(verse.Translation);
    }

    public static async Task CopyTransliterationRequested(
        TopLevel? topLevel,
        Verse verse)
    {
        var clipboard = topLevel?.Clipboard;

        if (clipboard != null)
            await clipboard.SetTextAsync(verse.Transliteration);
    }

    public static async Task CopyVerseRequested(
        TopLevel? topLevel,
        Verse verse)
    {
        var clipboard = topLevel?.Clipboard;

        if (clipboard != null)
            await clipboard.SetTextAsync(verse.Text);
    }

    public static async Task VerseComponentOnCopyAllRequested(
        TopLevel? topLevel,
        Verse verse)
    {
        var clipboard = topLevel?.Clipboard;

        if (clipboard != null)
        {
            var text = $"{verse.Text}\n{verse.Transliteration}\n{verse.Translation}";
            await clipboard.SetTextAsync(text);
        }
    }

    public static void OnBookmarkVerseRequested(Verse verse, Surah surah)
    {
        var bookmark = new Bookmark
        {
            SurahId = surah.Id,
            VerseId = verse.Id
        };
        if (DataManager.IsBookmarked(surah.Id, verse.Id))
            DataManager.RemoveBookmark(bookmark);
        else
            DataManager.AddBookmark(bookmark);
    }
}