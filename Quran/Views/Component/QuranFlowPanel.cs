using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace Quran.Views.Component;

public class QuranFlowPanel : Panel
{
    private readonly List<Line> _lines = new();

    protected override Size MeasureOverride(Size availableSize)
    {
        _lines.Clear();

        var availableWidth = availableSize.Width;

        if (double.IsInfinity(availableWidth) || availableWidth <= 0)
        {
            foreach (var child in Children) child.Measure(availableSize);

            return new Size(0, 0);
        }

        var currentLine = new Line();

        foreach (var child in Children)
        {
            // First measure naturally.
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));

            var desired = child.DesiredSize;

            // If the item itself is wider than the page,
            // constrain it so its internal TextBlock can wrap.
            if (desired.Width > availableWidth)
            {
                child.Measure(new Size(
                    availableWidth,
                    availableSize.Height));

                desired = child.DesiredSize;
            }

            // Doesn't fit on the current line.
            if (currentLine.Children.Count > 0 &&
                currentLine.Width + desired.Width > availableWidth)
            {
                _lines.Add(currentLine);
                currentLine = new Line();
            }

            currentLine.Children.Add(child);
            currentLine.Width += desired.Width;
            currentLine.Height = Math.Max(
                currentLine.Height,
                desired.Height);
        }

        if (currentLine.Children.Count > 0) _lines.Add(currentLine);

        var totalHeight = 0.0;

        foreach (var line in _lines) totalHeight += line.Height;

        return new Size(
            availableWidth,
            totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var y = 0.0;

        foreach (var line in _lines)
        {
            // Center each Quran line.
            var x = (finalSize.Width - line.Width) / 2.0;

            // RTL: start from the right side.
            foreach (var child in line.Children)
            {
                var width = child.DesiredSize.Width;

                child.Arrange(new Rect(
                    x,
                    y,
                    width,
                    line.Height));

                x += width;
            }

            y += line.Height;
        }

        return finalSize;
    }

    private sealed class Line
    {
        public List<Control> Children { get; } = new();
        public double Width { get; set; }
        public double Height { get; set; }
    }
}