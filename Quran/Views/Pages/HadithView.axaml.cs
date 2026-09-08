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
        HadithComboBox.ItemsSource = _hadithBooks.OrderBy(q=>q);
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
                // Do something with the hadiths, e.g., display them in a ListBox or other control
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

    }
}