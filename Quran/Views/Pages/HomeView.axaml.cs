using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;

namespace Quran.Views.Pages;

public partial class HomeView : AView
{
    private bool _isLoaded;
    private List<SurahOrder> _surahOrder = new();
    private List<SurahSynopsis> _surahSynopsis = new();
    private IEnumerable<Surah> _surahs = new List<Surah>();
    private Dictionary<int, SurahSynopsis> _synopsisLookup = new();

    public HomeView()
    {
        InitializeComponent();
    }

    private List<CardComponent> Cards { get; } = new();


    public override async Task Load(params object?[] parameter)
    {
        if (!_isLoaded)
        {
            // Detach source first; mutating the same List while it's still bound can break repeater bookkeeping.
            SurahRepeater.ItemsSource = null;

            foreach (var existingCard in Cards) existingCard.CardClick -= Card_CardClick;

            Cards.Clear();
            _surahs = DataManager.Surahs;
            _surahOrder = DataManager.SurahOrders;
            _surahSynopsis = DataManager.SurahSynopses;
            _synopsisLookup = DataManager.SurahSynopses.ToDictionary(x => x.SurahId);
            await GotoComponent.Load(_surahs, _surahOrder, _surahSynopsis);
            foreach (var surah in _surahs)
            {
                _synopsisLookup.TryGetValue(surah.Id, out var synopsis);
                var card = new CardComponent(surah, synopsis);
                card.CardClick += Card_CardClick;
                Cards.Add(card);
            }

            SurahRepeater.ItemsSource = Cards;
        }

        _isLoaded = true;

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

    public override async Task Reload(params object?[] parameter)
    {
        _isLoaded = false;
        await Load(parameter);
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
        var card = Cards.FirstOrDefault(c => c.Surah?.Id == surah.Id);
        if (card != null) Application.Current?.Dispatcher.Invoke(() => { SelectCard(card); });
    }

    private void SelectCard(CardComponent card)
    {
        foreach (var c in Cards) c.IsSelected = c.Surah?.Id == card.Surah?.Id;
    }
}