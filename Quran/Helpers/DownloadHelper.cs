using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Quran.Helpers;

public class DownloadParameter(string url, string destinationPath)
{
    public readonly string Url = url;
    public readonly string DestinationPath = destinationPath;
}

public static class DownloadHelper
{
    private static readonly HttpClient HttpClient = new();

    public static async Task DownloadFileAsync(DownloadParameter downloadParameter)
    {
        var url = downloadParameter.Url;
        var destinationPath = downloadParameter.DestinationPath;
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