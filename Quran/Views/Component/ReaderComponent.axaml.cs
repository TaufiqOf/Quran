using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Quran.Models;

namespace Quran.Views.Component;

public partial class ReaderComponent : UserControl,IAsyncDisposable
{
    public delegate void VerseSelectedEventHandler(Verse verse);
    public event VerseSelectedEventHandler? VerseSelected;
    
    public delegate void VersesLoadedEventHandler();
    public event VersesLoadedEventHandler? VersesLoaded;
    
    private readonly List<VerseComponent> _verseComponents = new();

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
        ItemsControl.Items.Clear();
    }

    public void AddVerses(IEnumerable<Verse> verses)
    {
        Task.Factory.StartNew(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var verse in verses)
                {
                    var verseComponent =
                        new VerseComponent(verse);

                    _verseComponents.Add(verseComponent);
                    verseComponent.VerseSelected += VerseComponent_OnVerseSelected;
                    ItemsControl.Items.Add(verseComponent);
                    
                }
                VersesLoaded?.Invoke();
            });
        });
    }

    private void VerseComponent_OnVerseSelected(Verse verse)
    {
        VerseSelected?.Invoke(verse);
    }

    public void BringVerseIntoView(int verseIndex)
    {
        if (verseIndex < 0 ||
            verseIndex > _verseComponents.Count)
        {
            return;
        }

        var verseComponent =
            _verseComponents[verseIndex - 1];
        // Wait until Avalonia has completed layout
        // before attempting to scroll.
        Dispatcher.UIThread.Post(
            () => { verseComponent.BringIntoView(); },
            DispatcherPriority.Loaded);
    }

    public void UpdateSelectedVerse(int verseIndex)
    {
        for (int i = 0; i < _verseComponents.Count; i++)
        {
            _verseComponents[i].IsSelected = i == verseIndex - 1;
        }
    }



    public async ValueTask DisposeAsync()
    {
        foreach (var verseComponent in _verseComponents)
        {
            verseComponent.VerseSelected -= VerseComponent_OnVerseSelected;
            verseComponent.Dispose();
        }
        CardComponent.Dispose();
    }
}