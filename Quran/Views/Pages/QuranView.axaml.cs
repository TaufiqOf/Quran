using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
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
    private bool _isLoaded = false;
    private int _currentSurahIndex = -1;

    public QuranView()
    {
        InitializeComponent();
        foreach (var value in Enum.GetValues(typeof(ReaderMode))) ModeComboBox.Items.Add(value.ToString());
        ModeComboBox.SelectedIndex = 0;
        AttachedToVisualTree += (_, _) =>
        {
            Dispatcher.UIThread.Post(
                () => ReaderComponent.Focus(),
                DispatcherPriority.Loaded);
        };
    }

    public override Task Load(params object?[] parameter)
    {
        if (!_isLoaded)
        {
            _surahs = DataManager.Surahs;
            _surahOrder = DataManager.SurahOrders;
            _surahSynopsis = DataManager.SurahSynopses;

            GotoComponent.Load(_surahs, _surahOrder, _surahSynopsis);
            _isLoaded = true;
        }

        if (parameter.Length == 0 || parameter[0] is null)
        {
            if (DataManager.CurrentSurah is not null)
            {
                var index = _surahs.ToList().FindIndex(q => q.Id == DataManager.CurrentSurah.Id);
                if (index >= 0) GotoComponent.SurahSelectedIndex = index;
            }
            else
            {
                GotoComponent.SurahSelectedIndex = 0;
            }

            if (DataManager.CurrentVerseIndex is not null)
                GotoComponent.VerseSelectedIndex = DataManager.CurrentVerseIndex.Value;
        }
        else if (parameter[0] is Surah surahParam)
        {
            var index = _surahs.ToList().FindIndex(q => q.Id == surahParam.Id);
            if (index >= 0) GotoComponent.SurahSelectedIndex = index;

            if (parameter.Length > 1 && parameter[1] is int verseIndexParam)
                GotoComponent.VerseSelectedIndex = verseIndexParam;
        }

        return Task.CompletedTask;
    }

    private void GotoComponent_OnSurahSelected(Surah surah)
    {
        if (_currentSurahIndex == surah.Id) return;

        if (DataManager.CurrentSurah?.Id != surah.Id) DataManager.CurrentVerseIndex = 1;

        DataManager.CurrentSurah = surah;

        var synopsis =
            _surahSynopsis.FirstOrDefault(q => q.SurahId == surah.Id);
        ReaderComponent.LoadCard(surah, synopsis);
        ReaderComponent.ClearVerses();
        ReaderComponent.AddVerses(surah.Verses);
    }


    private void GotoComponent_OnVerseSelected(int verseId)
    {
        DataManager.CurrentVerseIndex = verseId;
        ScrollToVerse(verseId);
        UpdateSelectedVerse(verseId);
    }


    private void ScrollToVerse(int verseIndex)
    {
        ReaderComponent.BringVerseIntoView(verseIndex);
    }

    private void UpdateSelectedVerse(int verseIndex)
    {
        ReaderComponent.UpdateSelectedVerse(verseIndex);
    }

    private void ReaderComponent_OnVerseSelected(Verse verse)
    {
        GotoComponent.VerseSelectedIndex = verse.Id;
    }

    private void ReaderComponent_OnVersesLoaded()
    {
        GotoComponent.VerseSelectedIndex = DataManager.CurrentVerseIndex is null
            ? 1
            : DataManager.CurrentVerseIndex == -1
                ? 1
                : DataManager.CurrentVerseIndex.Value;
        ScrollToVerse(GotoComponent.VerseSelectedIndex);
        UpdateSelectedVerse(GotoComponent.VerseSelectedIndex);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ReaderComponent.Mode = Enum.Parse<ReaderMode>(ModeComboBox.SelectionBoxItem.ToString());
    }


    private void ReaderComponent_OnKeyDown(object? sender, KeyEventArgs e)
    {
        GotoComponent.SetFocusOnVerse();
    }
}