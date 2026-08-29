using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;

namespace Quran.Views.Pages;

public partial class HomeView : AView
{
    public List<CardComponent> Cards { get; set; } = new();
    private IEnumerable<Surah> _surahs = new List<Surah>();
    private List<SurahOrder> _surahOrder;
    private List<SurahSynopsis> _surahSynopsis;
    private bool _isLoaded;

    public HomeView()
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
            foreach (var surah in _surahs)
            {
                var synopsis = _surahSynopsis.FirstOrDefault(q => q.SurahId == surah.Id);
                var card = new CardComponent(surah, synopsis);
                card.CardClick += Card_CardClick;
                Cards.Add(card);
                ItemsControl.Items.Add(card);
            }

            _isLoaded = true;
        }


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

        return Task.CompletedTask;
    }

    private void Card_CardClick(Surah surah)
    {
        if (DataManager.CurrentSurah?.Id == surah.Id)
        {
            RequestGotoPage("Quran", surah);
        }
        else
        {
            DataManager.CurrentSurah = surah;
            DataManager.CurrentVerseId = 1;
            RequestGotoPage("Quran", surah);
        }
    }

    private void GotoComponent_OnSurahSelected(Surah surah)
    {
        var card = Cards.FirstOrDefault(c => c.Surah!.Id == surah.Id);
        if (card != null)
        {
            Dispatcher.UIThread.Post(
                () => { card.BringIntoView(); },
                DispatcherPriority.Loaded);
            SelectCard(card);
        }
    }

    private void SelectCard(CardComponent card)
    {
        foreach (var c in Cards) c.IsSelected = c.Surah?.Id == card.Surah?.Id;
    }
}