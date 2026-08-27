using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;

namespace Quran.Views.Pages;

public partial class QuranView : AView
{
    private IEnumerable<Surah> _surahs = [];
    private IEnumerable<SurahOrder> _surahOrder = [];
    private IEnumerable<SurahSynopsis> _surahSynopsis = [];

    // Keep references to the VerseComponent controls.
    private readonly List<VerseComponent> _verseComponents = [];

    public QuranView()
    {
        InitializeComponent();
        PreviousButton.IsEnabled = false;
    }

    public override Task Load(object? parameter)
    {
        _surahs = GetData.GetSurahs();
        _surahOrder = GetData.SurahOrder();
        _surahSynopsis = GetData.SurahSynopsis();

        var surahTransliterations = GetData.GetSurahTransliterations();

        // Add transliterations to verses.
        foreach (var surahTransliteration in surahTransliterations)
        {
            var surah = _surahs.FirstOrDefault(
                q => q.Id == surahTransliteration.Id);

            if (surah != null)
            {
                foreach (var transliterationVerse in surahTransliteration.Verses)
                {
                    var verse = surah.Verses
                        .FirstOrDefault(v => v.Id == transliterationVerse.Id);

                    if (verse != null)
                    {
                        verse.Transliteration =
                            transliterationVerse.Transliteration;
                    }
                }
            }
        }

        // Populate Surah ComboBox.
        SurahComboBox.Items.Clear();

        foreach (var surah in _surahs)
        {
            SurahComboBox.Items.Add(
                new ComboBoxItem
                {
                    Content =
                        $"{surah.Id}. {surah.Transliteration} - {surah.Name}",
                    Tag = surah
                });
        }
        if(parameter is null)
        {
            SurahComboBox.SelectedIndex = 0;
        }
        else if(parameter is Surah surahParam)
        {
            var index = _surahs.ToList().FindIndex(q => q.Id == surahParam.Id);
            if(index >= 0)
            {
                SurahComboBox.SelectedIndex = index;
            }
        }

        return Task.CompletedTask;
    }


    private void SurahComboBox_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        var comboBoxItem =
            SurahComboBox.SelectedItem as ComboBoxItem;

        var surah =
            comboBoxItem?.Tag as Surah;

        if (surah == null)
            return;


        // ==========================================
        // Load Surah Card
        // ==========================================

        var synopsis =
            _surahSynopsis.FirstOrDefault(
                q => q.SurahId == surah.Id);

        CardComponent.LoadData(surah, synopsis);
        CardComponent.IsVisible = true;


        // ==========================================
        // Populate Verse ComboBox
        // ==========================================

        VerseComboBox.SelectionChanged -=
            VerseComboBox_OnSelectionChanged;

        VerseComboBox.Items.Clear();

        foreach (var verse in surah.Verses)
        {
            VerseComboBox.Items.Add(
                new ComboBoxItem
                {
                    Content = $"Verse {verse.Id}",
                    Tag = verse.Id
                });
        }

        VerseComboBox.SelectedIndex = 0;

        VerseComboBox.SelectionChanged +=
            VerseComboBox_OnSelectionChanged;


        // ==========================================
        // Create Verse Components
        // ==========================================

        ItemsControl.Items.Clear();
        foreach (var verseComponent in _verseComponents)
        {
            verseComponent.VerseSelected -= VerseComponent_OnVerseSelected;
        }
        _verseComponents.Clear();

        foreach (var verse in surah.Verses)
        {
            var verseComponent =
                new VerseComponent(verse);

            _verseComponents.Add(verseComponent);
            verseComponent.VerseSelected += VerseComponent_OnVerseSelected;
            ItemsControl.Items.Add(verseComponent);
        }


        // ==========================================
        // Scroll to first verse
        // ==========================================
        PreviousButton.IsEnabled = false;
        NextButton.IsEnabled = true;
        ScrollToVerse(0);
        UpdateSelectedVerse(0);
    }

    private void VerseComponent_OnVerseSelected(Verse verse)
    {
        VerseComboBox.SelectedIndex = verse.Id - 1;
    }


    private void VerseComboBox_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (VerseComboBox.SelectedIndex < 0)
            return;

        var selectedIndex =
            VerseComboBox.SelectedIndex;

        ScrollToVerse(selectedIndex);
        UpdateSelectedVerse(selectedIndex);
        
    }


    private void ScrollToVerse(int verseIndex)
    {
        if (verseIndex < 0 ||
            verseIndex >= _verseComponents.Count)
        {
            return;
        }

        var verseComponent =
            _verseComponents[verseIndex];
        // Wait until Avalonia has completed layout
        // before attempting to scroll.
        Dispatcher.UIThread.Post(
            () =>
            {
                verseComponent.BringIntoView();
            },
            DispatcherPriority.Loaded);
    }

    private void UpdateSelectedVerse(int verseIndex)
    {
        for (int i = 0; i < _verseComponents.Count; i++)
        {
            _verseComponents[i].IsSelected = i == verseIndex;
        }
    }
    
    private void PreviousButton_OnClick(object? sender, RoutedEventArgs e)
    {

        if(VerseComboBox.SelectedIndex > 0)
        {
            VerseComboBox.SelectedIndex--;
            NextButton.IsEnabled = true;
        }
        if (VerseComboBox.SelectedIndex == 0)
        {
            PreviousButton.IsEnabled = false;
        }
    }

    private void NextButton_OnClick(object? sender, RoutedEventArgs e)
    {

        if(VerseComboBox.SelectedIndex < VerseComboBox.Items.Count - 1)
        {
            VerseComboBox.SelectedIndex++;
            PreviousButton.IsEnabled = true;
        }
        if (VerseComboBox.SelectedIndex ==  VerseComboBox.Items.Count - 1)
        {
            NextButton.IsEnabled = false;
        }
    }

    private void PreviousSurahButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if(SurahComboBox.SelectedIndex > 0)
        {
            SurahComboBox.SelectedIndex--;
            NextSurahButton.IsEnabled = true;
        }
        if (SurahComboBox.SelectedIndex == 0)
        {
            PreviousSurahButton.IsEnabled = false;
        }
        PreviousButton.IsEnabled = false;
    }

    private void NextSurahButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if(SurahComboBox.SelectedIndex < SurahComboBox.Items.Count - 1)
        {
            SurahComboBox.SelectedIndex++;
            PreviousSurahButton.IsEnabled = true;
        }
        if (SurahComboBox.SelectedIndex == SurahComboBox.Items.Count - 1)
        {
            NextSurahButton.IsEnabled = false;
        }
        PreviousButton.IsEnabled = false;
    }
}