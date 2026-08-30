using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Pages;

public partial class QuranView : AView
{
    private readonly int _currentSurahIndex = -1;
    private bool _isLoaded;
    private IEnumerable<SurahOrder> _surahOrder = [];
    private IEnumerable<SurahSynopsis> _surahSynopsis = [];
    private IEnumerable<Surah> _surahs = [];

    public QuranView()
    {
        InitializeComponent();
        AudioComponent.PlayAction += PlayCurrentVerse;
        AudioComponent.PauseAction += PauseCurrentVerse;
        AudioComponent.SeekAction += SeekCurrentVerse;
        foreach (var value in Enum.GetValues(typeof(ReaderMode))) ModeComboBox.Items.Add(value.ToString());
        ModeComboBox.SelectedIndex = 0;
        AttachedToVisualTree += (_, _) =>
        {
            Dispatcher.UIThread.Post(
                () => ReaderComponent.Focus(),
                DispatcherPriority.Loaded);
        };
        try
        {
            AudioHelper.AudioEnded += AudioEnded;
        }
        catch (Exception e)
        {
            AudioComponent.PlayButton.IsEnabled = false;
        }
    }


    public override Task Load(params object?[] parameter)
    {
        if (parameter.Length == 1 && parameter[0] is object?[] nestedParameters) parameter = nestedParameters;

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

            if (DataManager.CurrentVerseId is not null)
                GotoComponent.VerseSelectedIndex = DataManager.CurrentVerseId.Value;
        }
        else if (parameter[0] is Surah surahParam)
        {
            var index = _surahs.ToList().FindIndex(q => q.Id == surahParam.Id);
            if (index >= 0) GotoComponent.SurahSelectedIndex = index;

            if (parameter.Length > 1 && parameter[1] is int verseIndexParam)
                GotoComponent.VerseSelectedIndex = verseIndexParam;
        }

        ReaderComponent.UpdateUi();
        return Task.CompletedTask;
    }

    private void GotoComponent_OnSurahSelected(Surah surah)
    {
        if (_currentSurahIndex == surah.Id) return;

        if (DataManager.CurrentSurah?.Id != surah.Id) DataManager.CurrentVerseId = 1;

        DataManager.CurrentSurah = surah;

        var synopsis =
            _surahSynopsis.FirstOrDefault(q => q.SurahId == surah.Id);
        ReaderComponent.LoadCard(surah, synopsis);
        ReaderComponent.ClearVerses();
        ReaderComponent.AddVerses(surah.Verses);
    }


    private void GotoComponent_OnVerseSelected(int verseId)
    {
        DataManager.CurrentVerseId = verseId;
        ScrollToVerse(verseId);
        UpdateSelectedVerse(verseId);
        if (AudioHelper.IsPlaying) AudioHelper.PlayAudio(DataManager.CurrentSurah?.Id ?? 1, verseId);
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
        GotoComponent.VerseSelectedIndex = DataManager.CurrentVerseId is null
            ? 1
            : DataManager.CurrentVerseId == -1
                ? 1
                : DataManager.CurrentVerseId.Value;
        ScrollToVerse(GotoComponent.VerseSelectedIndex);
        UpdateSelectedVerse(GotoComponent.VerseSelectedIndex);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItem = ModeComboBox?.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedItem))
            return;
        ReaderComponent.Mode = Enum.Parse<ReaderMode>(selectedItem);
    }


    private void ReaderComponent_OnKeyDown(object? sender, KeyEventArgs e)
    {
        GotoComponent.SetFocusOnVerse();
    }

    private void SeekCurrentVerse(double position)
    {
        AudioHelper.SeekAudio(position);
    }

    private void PauseCurrentVerse()
    {
        AudioHelper.PauseAudio();
    }

    private void PlayCurrentVerse()
    {
        AudioHelper.PlayAudio(DataManager.CurrentSurah?.Id ?? 1, DataManager.CurrentVerseId ?? 1);
    }

    private void AudioEnded()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var currentSurah = DataManager.CurrentSurah;
            if (currentSurah is null)
                return;

            var currentVerseId = DataManager.CurrentVerseId ?? 1;
            var nextVerseId = currentVerseId + 1;
            // Next verse in current Surah
            if (nextVerseId <= currentSurah.Verses.Count)
            {
                DataManager.CurrentVerseId = nextVerseId;
                GotoComponent.VerseSelectedIndex = nextVerseId;
                ScrollToVerse(nextVerseId);
                UpdateSelectedVerse(nextVerseId);
                AudioHelper.PlayAudio(currentSurah.Id, nextVerseId);
                return;
            }

            // Find next Surah
            var nextSurah = _surahs.OrderBy(s => s.Id).FirstOrDefault(s => s.Id > currentSurah.Id);

            // Finished last Surah
            if (nextSurah is null)
            {
                nextSurah = _surahs.OrderBy(s => s.Id).FirstOrDefault();
                if (nextSurah is null)
                    return;
            }

            // Update application state FIRST
            DataManager.CurrentSurah = nextSurah;
            DataManager.CurrentVerseId = 1;

            // Update UI
            GotoComponent.SurahSelectedIndex =
                _surahs
                    .ToList()
                    .FindIndex(s => s.Id == nextSurah.Id);

            GotoComponent.VerseSelectedIndex = 1;

            ReaderComponent.LoadCard(nextSurah, _surahSynopsis.FirstOrDefault(s => s.SurahId == nextSurah.Id));
            ReaderComponent.ClearVerses();
            ReaderComponent.AddVerses(nextSurah.Verses);
            ScrollToVerse(1);
            UpdateSelectedVerse(1);
            // Play explicitly with the new IDs
            AudioHelper.PlayAudio(nextSurah.Id, 1);
        });
    }
}