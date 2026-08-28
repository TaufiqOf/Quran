using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Quran.Models;

namespace Quran.Views.Component;

public partial class ReaderComponent : UserControl, IDisposable
{
    public delegate void VerseSelectedEventHandler(Verse verse);

    public event VerseSelectedEventHandler? VerseSelected;

    public delegate void VersesLoadedEventHandler();

    public event VersesLoadedEventHandler? VersesLoaded;

    private readonly List<AVerseComponent> _verseComponents = new();
    private IEnumerable<Verse> _verses = Array.Empty<Verse>();
    private ReaderMode _mode;


    public ReaderMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value)
            {
                return;
            }

            _mode = value;
            UpdateMode(_mode);
        }
    }

    private void UpdateMode(ReaderMode mode)
    {
        Task.Factory.StartNew(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                ClearVerses();
                foreach (var verse in _verses)
                {
                    AVerseComponent verseComponent = mode switch
                    {
                        ReaderMode.Linear => new VerseComponent(verse),
                        ReaderMode.Compact => new VerseCompactComponent(verse),
                        ReaderMode.Quranic => new VerseQuranicComponent(verse),
                        ReaderMode.Translation => new VerseTranslationComponent(verse),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    _verseComponents.Add(verseComponent);
                    verseComponent.VerseSelected += VerseComponent_OnVerseSelected;
                    LinerItemsControl.Items.Add(verseComponent);
                }

                VersesLoaded?.Invoke();
            });
        });
    }

    public ReaderComponent()
    {
        InitializeComponent();
    }

    public void LoadCard(Surah surah, SurahSynopsis? synopsis)
    {
        CardComponent.LoadData(surah, synopsis);
        CardComponent.IsVisible = true;
    }

    public void ClearVerses()
    {
        foreach (var verseComponent in _verseComponents)
        {
            verseComponent.VerseSelected -= VerseComponent_OnVerseSelected;
            verseComponent.Dispose();
        }

        _verseComponents.Clear();
        LinerItemsControl.Items.Clear();
    }

    public void AddVerses(IEnumerable<Verse> verses)
    {
        _verses = verses;
        UpdateMode(Mode);
    }

    private void VerseComponent_OnVerseSelected(Verse verse)
    {
        VerseSelected?.Invoke(verse);
    }

    public void BringVerseIntoView(int verseIndex)
    {
        var index = verseIndex - 1;

        if (index < 0 || index >= _verseComponents.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            LinerItemsControl.ScrollIntoView(index);
        }, DispatcherPriority.Loaded);
    }

    public void UpdateSelectedVerse(int verseIndex)
    {
        for (int i = 0; i < _verseComponents.Count; i++)
        {
            _verseComponents[i].IsSelected = i == verseIndex - 1;
        }
    }


    public void Dispose()
    {
        foreach (var verseComponent in _verseComponents)
        {
            verseComponent.VerseSelected -= VerseComponent_OnVerseSelected;
            verseComponent.Dispose();
        }

        CardComponent.Dispose();
    }
}