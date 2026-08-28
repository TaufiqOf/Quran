using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quran.Helpers;
using Quran.Models;

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
                if (index >= 0)
                {
                    GotoComponent.SurahSelectedIndex = index;
                }
            }
            else
            {
                GotoComponent.SurahSelectedIndex = 0;
            }

            if (DataManager.CurrentVerseIndex is not null)
            {
                GotoComponent.VerseSelectedIndex = DataManager.CurrentVerseIndex.Value;
            }
        }
        else if (parameter[0] is Surah surahParam)
        {
            var index = _surahs.ToList().FindIndex(q => q.Id == surahParam.Id);
            if (index >= 0)
            {
                GotoComponent.SurahSelectedIndex = index;
            }
            if(parameter.Length > 1 && parameter[1] is int verseIndexParam)
            {
                GotoComponent.VerseSelectedIndex = verseIndexParam;
            }
        }

        return Task.CompletedTask;
    }

    private void GotoComponent_OnSurahSelected(Surah surah)
    {
        if (_currentSurahIndex == surah.Id)
        {
            return;
        }
        if(DataManager.CurrentSurah?.Id != surah.Id)
        {
            DataManager.CurrentVerseIndex = 1;
        }
        DataManager.CurrentSurah = surah;
 
        var synopsis =
            _surahSynopsis.FirstOrDefault(q => q.SurahId == surah.Id);
        ReaderLineComponent.LoadCard(surah, synopsis);
        ReaderLineComponent.ClearVerses();
        ReaderLineComponent.AddVerses(surah.Verses);
    }


    private void GotoComponent_OnVerseSelected(int verseId)
    {
        DataManager.CurrentVerseIndex = verseId;
        ScrollToVerse(verseId);
        UpdateSelectedVerse(verseId);
    }



    private void ScrollToVerse(int verseIndex)
    {
        ReaderLineComponent.BringVerseIntoView(verseIndex);
    }

    private void UpdateSelectedVerse(int verseIndex)
    {
        ReaderLineComponent.UpdateSelectedVerse(verseIndex);

    }

    private void ReaderLineComponent_OnVerseSelected(Verse verse)
    {
        GotoComponent.VerseSelectedIndex = verse.Id;
    }
    private void ReaderLineComponent_OnVersesLoaded()
    {
        GotoComponent.VerseSelectedIndex = DataManager.CurrentVerseIndex is null
            ? 1
            : DataManager.CurrentVerseIndex == -1
                ? 1
                : DataManager.CurrentVerseIndex.Value;
        ScrollToVerse(GotoComponent.VerseSelectedIndex);
        UpdateSelectedVerse(GotoComponent.VerseSelectedIndex);
    }

}