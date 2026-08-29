using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Quran.Models;

namespace Quran.Views.Component;

public partial class VerseQuranicComponent : AVerseComponent, IDisposable
{
    public VerseQuranicComponent(Surah surah, Verse verse) : base(surah, verse)
    {
        Verse = verse;

        InitializeComponent();
        TextBlockVerseNumber.Text = $"۝{ToArabicDigits(verse.Id)}";
        TextBlockVerseNumber.Foreground = IsBookMarked ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.White);
        TextBlockArabic.Text = $"{verse.Text} ";
    }

    private static string ToArabicDigits(int number)
    {
        return number.ToString()
            .Replace('0', '٠')
            .Replace('1', '١')
            .Replace('2', '٢')
            .Replace('3', '٣')
            .Replace('4', '٤')
            .Replace('5', '٥')
            .Replace('6', '٦')
            .Replace('7', '٧')
            .Replace('8', '٨')
            .Replace('9', '٩');
    }

    public override void UpdateSelectedState()
    {
        VerseCard.Classes.Set(
            "selected",
            IsSelected);
        TextBlockArabic.Classes.Set(
            "selected",
            IsSelected);
    }


    private void VerseCard_OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        OnVerseSelected(Verse);
    }


    public override void VerseBookMark()
    {
        TextBlockVerseNumber.Foreground = IsBookMarked ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.White);
    }

    public override void Dispose()
    {
        Verse = null;
    }
}