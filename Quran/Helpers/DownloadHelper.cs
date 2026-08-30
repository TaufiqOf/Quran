using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Quran.Helpers;

public static class DownloadHelper
{
    private static readonly HttpClient HttpClient = new();

    public static async Task DownloadFileAsync(string url, string destinationPath)
    {
        using var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
    }
}