using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Pages;

public partial class HadithView : AView
{
    private List<string?> _hadithBooks;

    public HadithView()
    {
        InitializeComponent();
    }

    public override Task Load(params object?[] parameter)
    {
        _hadithBooks = DataManager.GetHadithBooks();
        HadithComboBox.ItemsSource = _hadithBooks.OrderBy(q => q);
        HadithComboBox.SelectedIndex = 0; // Optionally select the first book by default
        return Task.CompletedTask;
    }

    public override Task Reload(params object?[] parameter)
    {
        return Task.CompletedTask;
    }

    private void HadithComboBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (HadithComboBox.SelectedItem is string selectedBook)
        {
            var chaptersByBooks = DataManager.GetHadithChaptersByBooks(selectedBook);
            HadithChapterComboBox.ItemsSource = chaptersByBooks.OrderBy(int.Parse).ToList();
            HadithChapterComboBox.SelectedIndex = 0; // Optionally select the first chapter by default
        }
    }

    private void HadithChapterComboBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (HadithComboBox.SelectedItem is string selectedBook &&
                HadithChapterComboBox.SelectedItem is string selectedChapter)
            {
                var hadithObject = DataManager.GetHadithsByBookAndChapter(selectedBook, selectedChapter);
                ItemsControl.ItemsSource = hadithObject?.Hadiths ?? Array.Empty<Hadith>();
                VerseChapterComboBox.ItemsSource =
                    hadithObject?.Hadiths.Select(h => h.Id.ToString()).Distinct().ToList() ?? new List<string>();
                VerseChapterComboBox.SelectedIndex = 0;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private void VerseChapterComboBoxOnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (VerseChapterComboBox.SelectedItem is not string selectedVerse)
            return;

        var hadith = ItemsControl.Items
            .OfType<Hadith>()
            .FirstOrDefault(h => h.Id.ToString() == selectedVerse);

        if (hadith == null)
            return;

        var index = ItemsControl.Items.IndexOf(hadith);

        if (index >= 0)
        {
            ItemsControl.ScrollIntoView(index);
        }
    }

    private void ItemsControlOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ItemsControl.SelectedItem is Hadith selectedHadith)
        {
            VerseChapterComboBox.SelectedIndex = VerseChapterComboBox.Items.IndexOf(selectedHadith.Id.ToString());
        }
    }
}