using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component.MessageControl;

namespace Quran.Views.Component;

public partial class ReaderComponent : UserControl, IDisposable
{
    public delegate void VerseSelectedEventHandler(Verse verse);

    public delegate void VersesLoadedEventHandler();

    private readonly List<AVerseComponent> _verseComponents = new();
    private VerseMessageControl? _control;
    private ReaderMode _mode;
    private Surah? _surah;
    private IEnumerable<Verse> _verses = Array.Empty<Verse>();

    public ReaderComponent()
    {
        InitializeComponent();
    }

    public Action<Verse>? PlayVerseRequested { get; set; }


    public ReaderMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value) return;

            _mode = value;
            UpdateMode(_mode);
        }
    }


    public void Dispose()
    {
        foreach (var verseComponent in _verseComponents)
        {
            verseComponent.VerseSelected -= VerseComponentOnVerseSelected;
            verseComponent.Dispose();
        }
    }

    public event VerseSelectedEventHandler? VerseSelected;

    public event VersesLoadedEventHandler? VersesLoaded;

    private void UpdateMode(ReaderMode mode)
    {
        Task.Factory.StartNew(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_surah == null) return;
                ClearVerses();
                if (mode == ReaderMode.Quranic)
                {
                    LinerScrollViewer.IsVisible = false;
                    QuranicScrollViewer.IsVisible = true;
                    var topLevel = TopLevel.GetTopLevel(this);

                    foreach (var verse in _verses)
                    {
                        var verseComponent = new VerseQuranicComponent(_surah, verse);
                        _verseComponents.Add(verseComponent);
                        verseComponent.VerseSelected += VerseComponentOnVerseSelected;
                        verseComponent.CopyTranslationRequested += async v =>
                            await ContextMenuHelper.CopyTranslationRequested(topLevel, v);
                        verseComponent.CopyTransliterationRequested += async v =>
                            await ContextMenuHelper.CopyTransliterationRequested(topLevel, v);
                        verseComponent.CopyVerseRequested += async v =>
                            await ContextMenuHelper.CopyVerseRequested(topLevel, v);
                        verseComponent.CopyAllRequested += async (s, v) =>
                            await ContextMenuHelper.VerseComponentOnCopyAllRequested(topLevel, s, v);
                        verseComponent.BookmarkVerseRequested += VerseComponentOnBookmarkVerseRequested;
                        QuranicItemsControl.Items.Add(verseComponent);
                    }
                }
                else
                {
                    LinerScrollViewer.IsVisible = true;
                    QuranicScrollViewer.IsVisible = false;
                    var topLevel = TopLevel.GetTopLevel(this);
                    foreach (var verse in _verses)
                    {
                        AVerseComponent verseComponent = mode switch
                        {
                            ReaderMode.Linear => new VerseComponent(_surah, verse),
                            ReaderMode.Compact => new VerseCompactComponent(_surah, verse),
                            ReaderMode.Translation => new VerseTranslationComponent(_surah, verse),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        _verseComponents.Add(verseComponent);
                        verseComponent.VerseSelected += VerseComponentOnVerseSelected;
                        verseComponent.CopyTranslationRequested += async v =>
                            await ContextMenuHelper.CopyTranslationRequested(topLevel, v);
                        verseComponent.CopyTransliterationRequested += async v =>
                            await ContextMenuHelper.CopyTransliterationRequested(topLevel, v);
                        verseComponent.CopyVerseRequested += async v =>
                            await ContextMenuHelper.CopyVerseRequested(topLevel, v);
                        verseComponent.CopyAllRequested += async (s, v) =>
                            await ContextMenuHelper.VerseComponentOnCopyAllRequested(topLevel, s, v);
                        verseComponent.TafasirRequested += VerseComponentOnTafasirRequested;
                        verseComponent.BookmarkVerseRequested += VerseComponentOnBookmarkVerseRequested;
                        verseComponent.PlayVerseRequested += VerseComponentOnPlayVerseRequested;
                        LinerItemsControl.Items.Add(verseComponent);
                    }
                }

                VersesLoaded?.Invoke();
            });
        });
    }

    private async void VerseComponentOnTafasirRequested(Surah? surah, Verse verse)
    {
        if (surah == null) return;
        var text = await DataManager.GetTafsirAsync(surah.Id, verse.Id);
        if (_control != null && MessageHelper.IsShowing) MessageHelper.Close();

        _control = new VerseMessageControl(new List<VerseMessageModel> { new(surah, verse, text) });

        MessageHelper.ShowMessage("Tafasir", _control, false);
    }

    private void VerseComponentOnPlayVerseRequested(Verse verse)
    {
        PlayVerseRequested?.Invoke(verse);
    }

    private void VerseComponentOnBookmarkVerseRequested(Verse verse, Surah surah)
    {
        ContextMenuHelper.OnBookmarkVerseRequested(verse, surah);
        var verseComponent = _verseComponents.FirstOrDefault(q => q.Surah?.Id == surah.Id && q.Verse?.Id == verse.Id);
        verseComponent?.VerseBookMark();
    }

    public void LoadCard(Surah surah, SurahSynopsis? synopsis)
    {
        _surah = surah;
        CardComponent.LoadData(surah, synopsis);
        CardComponent.IsVisible = true;
    }

    public void ClearVerses()
    {
        foreach (var verseComponent in _verseComponents)
        {
            verseComponent.VerseSelected -= VerseComponentOnVerseSelected;
            verseComponent.Dispose();
        }

        _verseComponents.Clear();
        LinerItemsControl.Items.Clear();
        QuranicItemsControl.Items.Clear();
    }

    public void AddVerses(IEnumerable<Verse> verses)
    {
        _verses = verses;
        UpdateMode(Mode);
    }

    private void VerseComponentOnVerseSelected(Verse verse)
    {
        VerseSelected?.Invoke(verse);
    }

    public void BringVerseIntoView(int verseIndex)
    {
        var index = verseIndex - 1;

        if (index < 0 || index >= _verseComponents.Count) return;

        if (Mode != ReaderMode.Quranic)
            Dispatcher.UIThread.Post(() => { LinerItemsControl.ScrollIntoView(index); }, DispatcherPriority.Loaded);
        else
            Dispatcher.UIThread.Post(() => { QuranicItemsControl.ScrollIntoView(index); }, DispatcherPriority.Loaded);
    }

    public void UpdateSelectedVerse(int verseIndex)
    {
        for (var i = 0; i < _verseComponents.Count; i++) _verseComponents[i].IsSelected = i == verseIndex - 1;
    }

    public void UpdateUi()
    {
        _verseComponents.ForEach(verseComponent => verseComponent.UpdateUi());
    }
}