using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Models;

namespace Quran.Views.Component;

public partial class GotoComponent : UserControl
{
    public delegate void SurahSelectedHandler(Surah surah);

    public delegate void VerseSelectedHandler(int verseId);

    private IEnumerable<Surah> _surahs = [];


    public GotoComponent()
    {
        InitializeComponent();
        PreviousButton.IsEnabled = false;
    }

    public int VerseSelectedIndex
    {
        get => VerseComboBox.SelectedIndex + 1;
        set => VerseComboBox.SelectedIndex = value - 1;
    }

    public bool ShowOnlySurah
    {
        get;
        set
        {
            VerseLabel.IsVisible = !value;
            PreviousButton.IsVisible = !value;
            NextButton.IsVisible = !value;
            VerseComboBox.IsVisible = !value;
            field = value;
        }
    }

    public int SurahSelectedIndex
    {
        get => SurahComboBox.SelectedIndex;
        set => SurahComboBox.SelectedIndex = value;
    }

    public event SurahSelectedHandler? SurahSelected;

    public event VerseSelectedHandler? VerseSelected;

    public Task Load(IEnumerable<Surah> surahs, IEnumerable<SurahOrder> surahOrder,
        IEnumerable<SurahSynopsis> surahSynopsis)
    {
        _surahs = surahs;

        // Populate Surah ComboBox.
        SurahComboBox.Items.Clear();

        foreach (var surah in _surahs)
            SurahComboBox.Items.Add(
                new ComboBoxItem
                {
                    Content =
                        $"{surah.Id}. {surah.Transliteration} ({surah.TotalVerses}) - {surah.Name}",
                    Tag = surah
                });


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


        VerseComboBox.Items.Clear();

        foreach (var verse in surah.Verses)
            VerseComboBox.Items.Add(
                new ComboBoxItem
                {
                    Content = $"Verse {verse.Id}",
                    Tag = verse.Id
                });


        PreviousButton.IsEnabled = false;
        NextButton.IsEnabled = true;
        PreviousSurahButton.IsEnabled = SurahComboBox.SelectedIndex > 0;
        if (SurahComboBox.SelectedIndex > 0) NextSurahButton.IsEnabled = true;

        if (SurahComboBox.SelectedIndex == 0) PreviousSurahButton.IsEnabled = false;

        if (SurahComboBox.SelectedIndex < SurahComboBox.Items.Count - 1) PreviousSurahButton.IsEnabled = true;

        if (SurahComboBox.SelectedIndex == SurahComboBox.Items.Count - 1) NextSurahButton.IsEnabled = false;

        PreviousButton.IsEnabled = false;
        SurahSelected?.Invoke(surah);
    }

    private void VerseComboBox_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e)
    {
        if (VerseComboBox.SelectedIndex < 0)
            return;

        var selectedIndex =
            VerseComboBox.SelectedIndex;
        if (VerseComboBox.SelectedIndex > 0) PreviousButton.IsEnabled = true;

        if (VerseComboBox.SelectedIndex == 0) PreviousButton.IsEnabled = false;

        if (VerseComboBox.SelectedIndex < VerseComboBox.Items.Count - 1) NextButton.IsEnabled = true;

        if (VerseComboBox.SelectedIndex == VerseComboBox.Items.Count - 1) NextButton.IsEnabled = false;

        VerseSelected?.Invoke(selectedIndex + 1);
    }

    private void PreviousButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (VerseComboBox.SelectedIndex > 0)
        {
            VerseComboBox.SelectedIndex--;
            NextButton.IsEnabled = true;
        }

        if (VerseComboBox.SelectedIndex == 0) PreviousButton.IsEnabled = false;
    }

    private void NextButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (VerseComboBox.SelectedIndex < VerseComboBox.Items.Count - 1)
        {
            VerseComboBox.SelectedIndex++;
            PreviousButton.IsEnabled = true;
        }

        if (VerseComboBox.SelectedIndex == VerseComboBox.Items.Count - 1) NextButton.IsEnabled = false;
    }

    private void PreviousSurahButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SurahComboBox.SelectedIndex > 0)
        {
            SurahComboBox.SelectedIndex--;
            NextSurahButton.IsEnabled = true;
        }

        if (SurahComboBox.SelectedIndex == 0) PreviousSurahButton.IsEnabled = false;

        PreviousButton.IsEnabled = false;
    }

    private void NextSurahButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SurahComboBox.SelectedIndex < SurahComboBox.Items.Count - 1)
        {
            SurahComboBox.SelectedIndex++;
            PreviousSurahButton.IsEnabled = true;
        }

        if (SurahComboBox.SelectedIndex == SurahComboBox.Items.Count - 1) NextSurahButton.IsEnabled = false;

        PreviousButton.IsEnabled = false;
    }

    public void SetFocusOnVerse()
    {
        VerseComboBox.Focus();
    }
}