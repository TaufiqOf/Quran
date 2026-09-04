using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Quran.Helpers;
using Quran.Models;
using System.Xml.Linq;

namespace Quran.Views.Pages;

public partial class AboutView : AView
{
    public AboutView()
    {
        InitializeComponent();
        VersionTextBlock.Text = GetVersionText();
        LicenseTextBlock.Text = GetLicenseText();
        TranslatorsItemsControl.ItemsSource = BuildCredits();
        PackagesItemsControl.ItemsSource = LoadPackages();
        SearchTipsItemsControl.ItemsSource = BuildSearchTips();
        AiAnswerItemsControl.ItemsSource = BuildAiAnswerDescription();
        DisclaimerItemsControl.ItemsSource = BuildDisclaimerItems();
    }

    public override async Task Load(params object?[] parameter)
    {
        await Task.CompletedTask;
    }

    public override async Task Reload(params object?[] parameter)
    {
        await Task.CompletedTask;
    }

    private static List<AboutCreditItem> BuildCredits()
    {
        var credits = new List<AboutCreditItem>
        {
            new() { Label = "Developed by", Value = "Taufiq A. Rahman" },
            new() { Label = "Audio recitation", Value = "Mahmoud Khalil Al-Husary" }
        };

        credits.AddRange(DataManager.Translators().Select(t => new AboutCreditItem
        {
            Label = $"{t.Language} translation",
            Value = t.Name
        }));

        return credits;
    }

    private static List<AboutTextItem> BuildDisclaimerItems()
    {
        return new List<AboutTextItem>
        {
            new()
            {
                Text = "This application is provided as-is for personal, educational, and non-commercial use. Quran text, translations, audio, and tafsir remain the property of their respective authors, publishers, and providers."
            },
            new()
            {
                Text = "Translation credits are shown for transparency. Please refer to the original translators or publishers for copyright, licensing, and attribution requirements related to each translation."
            }
        };
    }

    private static List<AboutTextItem> BuildSearchTips()
    {
        return new List<AboutTextItem>
        {
            new() { Text = "🔍 Search for a word, such as Jesus. Search can also find close spellings and related wording." },
            new() { Text = "📖 Open a specific verse: >2:255" },
            new() { Text = "📚 Open a range of verses: >2:2-5" },
            new() { Text = "📖 Open an entire chapter: >112:" },
            new() { Text = "❓ Ask a question in the Ask AI page, such as: Who will go to Heaven?" },
            new() { Text = "🔎 Use several words together to narrow your search: Jesus Mary" },
            new() { Text = "💡 Try different or simpler words if you do not find what you need." },
            new() { Text = "🔤 Search English translations, transliteration, or Arabic text directly." },
            new() { Text = "🎯 Search for a meaning or concept with ?: ? reward for good deeds" },
            new() { Text = "🌐 Meaning-based searches can be written in any language: ? What is the night of decree?" },
            new() { Text = "🎯 Choose the number of meaning-based results with :N: ? mercy:5" },
            new() { Text = "🔖 Find a surah by name: >Baqarah: or jump by number: >114:" }
        };
    }

    private static List<AboutTextItem> BuildAiAnswerDescription()
    {
        return new List<AboutTextItem>
        {
            new() { Text = "Ask AI gives a concise answer based only on the Quran verses found for your question. It does not use outside information." },
            new() { Text = "Each answer includes the surah and verse references used as its sources, so you can read the verses yourself." },
            new() { Text = "When the available verses do not clearly answer the question, it says: \"I cannot answer this query based on the provided context.\"" },
            new() { Text = "AI answers are study aids. Please read the cited verses in their full context." }
        };
    }

    private static string GetVersionText()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(AboutView).Assembly;

        var version = assembly.GetName().Version;
        return version is null
            ? "Version unavailable"
            : $"Version {version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
    }

    private static string GetLicenseText()
    {
        var licenseFile = FindFileUpwards("LICENSE");
        if (!string.IsNullOrWhiteSpace(licenseFile) && File.Exists(licenseFile))
        {
            var firstLine = File.ReadLines(licenseFile).FirstOrDefault()?.Trim();
            if (string.Equals(firstLine, "GNU GENERAL PUBLIC LICENSE", StringComparison.OrdinalIgnoreCase))
                return $"© 2026-{DateTime.Now.Year} Quran Application Contributors. Licensed under the GNU General Public License v3.0.";
        }

        return $"© 2026-{DateTime.Now.Year} Quran Application Contributors.";
    }

    private static List<AboutPackageItem> LoadPackages()
    {
        var packages = LoadPackagesFromProjectFile();
        if (packages.Count > 0)
            return packages;

        packages = LoadPackagesFromDepsFile();
        return packages;
    }

    private static List<AboutPackageItem> LoadPackagesFromProjectFile()
    {
        var projectFile = FindFileUpwards("Quran.csproj");
        if (string.IsNullOrWhiteSpace(projectFile) || !File.Exists(projectFile))
            return new List<AboutPackageItem>();

        var document = XDocument.Load(projectFile);
        return document
            .Descendants()
            .Where(node => node.Name.LocalName == "PackageReference")
            .Select(node => new AboutPackageItem
            {
                Name = node.Attribute("Include")?.Value ?? string.Empty,
                Version = node.Attribute("Version")?.Value ?? string.Empty
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Version))
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<AboutPackageItem> LoadPackagesFromDepsFile()
    {
        var assemblyName = (Assembly.GetEntryAssembly() ?? typeof(AboutView).Assembly).GetName().Name ?? "Quran";
        var depsFile = Directory
            .EnumerateFiles(AppContext.BaseDirectory, "*.deps.json", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(depsFile) || !File.Exists(depsFile))
            return new List<AboutPackageItem>();

        using var stream = File.OpenRead(depsFile);
        using var document = JsonDocument.Parse(stream);

        if (!document.RootElement.TryGetProperty("targets", out var targetsElement))
            return new List<AboutPackageItem>();

        var targetElement = targetsElement.EnumerateObject().FirstOrDefault().Value;
        if (targetElement.ValueKind == JsonValueKind.Undefined)
            return new List<AboutPackageItem>();

        var appTarget = targetElement.EnumerateObject()
            .FirstOrDefault(property => property.Name.StartsWith(assemblyName + "/", StringComparison.OrdinalIgnoreCase));

        if (appTarget.Value.ValueKind == JsonValueKind.Undefined ||
            !appTarget.Value.TryGetProperty("dependencies", out var dependenciesElement))
            return new List<AboutPackageItem>();

        return dependenciesElement.EnumerateObject()
            .Select(property => new AboutPackageItem
            {
                Name = property.Name,
                Version = property.Value.GetString() ?? string.Empty
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Version))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? FindFileUpwards(string fileName)
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var candidate = Path.Combine(currentDirectory.FullName, fileName);
            if (File.Exists(candidate))
                return candidate;

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}