using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;

namespace Quran.Helpers;

public static class JsonReader
{
    public static string ReadJsonFromResource(string resourceName)
    {
        var assemblyName = typeof(JsonReader)
            .Assembly
            .GetName()
            .Name;

        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new InvalidOperationException(
                "Could not determine assembly name.");

        var uri = new Uri(
            $"avares://{assemblyName}/Data/{resourceName}");

        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static List<T> ReadJsonList<T>(string json)
    {
        return JsonSerializer.Deserialize<List<T>>(json)
               ?? new List<T>();
    }

    public static T? ReadJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }
}