namespace Quran.Models;

public sealed class AboutCreditItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class AboutPackageItem
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DisplayText => $"• {Name} {Version}";
}

public sealed class AboutTextItem
{
    public string Text { get; set; } = string.Empty;
}

