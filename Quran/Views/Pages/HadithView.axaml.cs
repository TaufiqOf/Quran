using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FuzzySharp;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Pages;

public partial class HadithView : AView
{
    private List<string?> _hadithBooks;
    private List<Hadith> _hadiths;
    private readonly ObservableCollection<Hadith> _hadithFiltered = new();

    public HadithView()
    {
        InitializeComponent();
        _hadiths = new List<Hadith>();
        ItemsControl.ItemsSource = _hadithFiltered;
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
            HadithChapterComboBox.ItemsSource =
                chaptersByBooks.Where(c => int.TryParse(c, out _)).OrderBy(int.Parse).ToList();
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
                _hadiths = hadithObject?.Hadiths?.ToList() ?? new List<Hadith>();
                _hadithFiltered.Clear();
                if (hadithObject?.Hadiths != null)
                {
                    foreach (var hadith in hadithObject.Hadiths)
                    {
                        _hadithFiltered.Add(hadith);
                    }
                }

                VerseChapterComboBox.ItemsSource =
                    _hadithFiltered.Select(h => h.Id.ToString()).Distinct().ToList() ?? new List<string>();
                if (VerseChapterComboBox.ItemCount > 0)
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
    private void SearchButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var query = SearchTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(query))
        {
            _hadithFiltered.Clear();
            foreach (var hadith in _hadiths)
            {
                _hadithFiltered.Add(hadith);
            }

            return;
        }

        var filteredHadithScore = _hadiths.Select(item => new
            {
                Item = item,
                Score = GetFuzzyScore(query, item)
            })
            .Where(x => x.Score >= FuzzyThreshold)
            .OrderBy(x => x.Item.Id);
        var filteredHadiths = filteredHadithScore.Select(x => x.Item).ToList();
        _hadithFiltered.Clear();
        foreach (var hadith in filteredHadiths)
        {
            _hadithFiltered.Add(hadith);
        }

        VerseChapterComboBox.ItemsSource =
            _hadithFiltered.Select(h => h.Id.ToString()).Distinct().ToList() ?? new List<string>();
        if (VerseChapterComboBox.ItemCount > 0)
            VerseChapterComboBox.SelectedIndex = 0;
    }


    public int FuzzyThreshold { get; set; } = 60;

    private static int GetFuzzyScore(string query, Hadith item)
    {
        var text = item.English.Text;

        var bestScore = Fuzz.PartialRatio(query, text);


        return bestScore;
    }

  
}