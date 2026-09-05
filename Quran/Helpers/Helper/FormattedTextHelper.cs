using System;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Quran.Helpers.Helper;

public static class FormattedTextHelper
{
    public static readonly AttachedProperty<string?> FormattedTextProperty =
        AvaloniaProperty.RegisterAttached<SelectableTextBlock, string?>(
            "FormattedText",
            typeof(FormattedTextHelper));

    public static void SetFormattedText(SelectableTextBlock element, string? value)
        => element.SetValue(FormattedTextProperty, value);

    public static string? GetFormattedText(SelectableTextBlock element)
        => element.GetValue(FormattedTextProperty);

    static FormattedTextHelper()
    {
        FormattedTextProperty.Changed.AddClassHandler<SelectableTextBlock>((control, e) =>
        {
            control.Inlines?.Clear();
            var text = e.GetNewValue<string>();

            if (string.IsNullOrEmpty(text))
                return;

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // 1. Headers (### Header)
                if (line.StartsWith("#"))
                {
                    int level = 0;
                    while (level < line.Length && line[level] == '#') level++;

                    var headerText = line.Substring(level).TrimStart();
                    var headerRun = new Run { Text = headerText, FontWeight = FontWeight.Bold };

                    // Adjust font size based on header level
                    switch (level)
                    {
                        case 1: headerRun.FontSize = (control.FontSize > 0 ? control.FontSize : 14) * 1.5; break;
                        case 2: headerRun.FontSize = (control.FontSize > 0 ? control.FontSize : 14) * 1.3; break;
                        default: headerRun.FontSize = (control.FontSize > 0 ? control.FontSize : 14) * 1.1; break;
                    }

                    control.Inlines?.Add(headerRun);
                }
                else
                {
                    // 2. Bullet Points (- or *)
                    if (Regex.IsMatch(line, @"^\s*[\-\*]\s+"))
                    {
                        var bulletMatch = Regex.Match(line, @"^\s*[\-\*]\s+");
                        control.Inlines?.Add(new Run { Text = "• ", FontWeight = FontWeight.Bold });
                        line = line.Substring(bulletMatch.Length);
                    }

                    // Parse inline markdown elements (Bold, Italic, Code, Links)
                    ParseInlineMarkdown(line, control);
                }

                // Append newline between lines
                if (i < lines.Length - 1)
                {
                    control.Inlines?.Add(new LineBreak());
                }
            }
        });
    }

    private static void ParseInlineMarkdown(string input, SelectableTextBlock control)
    {
        // Pattern matches:
        // Group 1: **bold**
        // Group 2: *italic* or _italic_
        // Group 3: `code`
        // Group 4: [link text](url)
        var pattern = @"(\*\*.*?\*\*)|(\*.*?\*|_.*?_)|(`.*?`)|(\[.*?\]\(.*?\))";
        var parts = Regex.Split(input, pattern);

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            // **Bold**
            if (part.StartsWith("**") && part.EndsWith("**") && part.Length >= 4)
            {
                control.Inlines?.Add(new Run
                {
                    Text = part.Substring(2, part.Length - 4),
                    FontWeight = FontWeight.Bold
                });
            }
            // *Italic* or _Italic_
            else if ((part.StartsWith("*") && part.EndsWith("*") && part.Length >= 2) ||
                     (part.StartsWith("_") && part.EndsWith("_") && part.Length >= 2))
            {
                control.Inlines?.Add(new Run
                {
                    Text = part.Substring(1, part.Length - 2),
                    FontStyle = FontStyle.Italic
                });
            }
            // `Code`
            else if (part.StartsWith("`") && part.EndsWith("`") && part.Length >= 2)
            {
                control.Inlines?.Add(new Run
                {
                    Text = part.Substring(1, part.Length - 2),
                    FontFamily = new FontFamily("Consolas, Courier New, Monospace"),
                    Background = new SolidColorBrush(Color.Parse("#2D2D2D")),
                    Foreground = new SolidColorBrush(Color.Parse("#CE9178"))
                });
            }
            // [Link Text](URL)
            else if (part.StartsWith("[") && part.Contains("](") && part.EndsWith(")"))
            {
                var match = Regex.Match(part, @"\[(.*?)\]\((.*?)\)");
                if (match.Success)
                {
                    var linkText = match.Groups[1].Value;
                    var url = match.Groups[2].Value;

                    var linkRun = new Run
                    {
                        Text = linkText,
                        Foreground = new SolidColorBrush(Color.Parse("#3794FF")),
                        TextDecorations = TextDecorations.Underline
                    };

                    control.Inlines?.Add(linkRun);
                }
            }
            // Plain Text
            else
            {
                control.Inlines?.Add(new Run { Text = part });
            }
        }
    }
}