using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Quran.Helpers;

public static class DownloadHelper
{
    private static readonly HttpClient HttpClient = new();

    public static async Task DownloadFileAsync(string url, string destinationPath)
    {
        var directoryPath = Path.GetDirectoryName(destinationPath);
        if (directoryPath == null)
            throw new DirectoryNotFoundException(
                $"The directory for the destination path '{destinationPath}' could not be determined.");
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        using var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

        await response.Content.CopyToAsync(fileStream);
    }
}