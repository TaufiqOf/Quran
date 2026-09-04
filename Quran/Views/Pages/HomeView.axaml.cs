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
    private bool _isLoaded;
    private int? _selectedSurahId;
    private List<SurahOrder> _surahOrder = new();
    private List<SurahSynopsis> _surahSynopsis = new();
    private IEnumerable<Surah> _surahs = new List<Surah>();
    private Dictionary<int, SurahSynopsis> _synopsisLookup = new();

    public HomeView()
    {
        InitializeComponent();
        CardComponent.CardClick += Card_CardClick;
    }

    private List<SurahCardViewModel> Cards { get; } = new();

    public override async Task Load(params object?[] parameter)
    {
        await Task.Run(async () =>
        {
            _surahs = DataManager.Surahs;
            _surahOrder = DataManager.SurahOrders;
            _surahSynopsis = DataManager.SurahSynopses;
            _synopsisLookup = DataManager.SurahSynopses.ToDictionary(x => x.SurahId);

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (!_isLoaded)
                {
                    SurahRepeater.ItemsSource = null;
                    Cards.Clear();

                    await GotoComponent.Load(_surahs, _surahOrder, _surahSynopsis);

                    foreach (var surah in _surahs)
                    {
                        _synopsisLookup.TryGetValue(surah.Id, out var synopsis);
                        Cards.Add(new SurahCardViewModel(surah, synopsis));
                    }

                    SurahRepeater.ItemsSource = Cards;
                }

                _isLoaded = true;
                //if (_selectedSurahId is not null && _selectedSurahId == DataManager.CurrentVerseId) return;

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
            });
        });
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
        var cardVm = Cards.FirstOrDefault(c => c.Surah?.Id == surah.Id);
        if (cardVm != null) SelectCard(cardVm);
    }

    private void SelectCard(SurahCardViewModel cardVm)
    {
        if (cardVm?.Surah?.Id == _selectedSurahId) return;
        _selectedSurahId = cardVm?.Surah?.Id;

        foreach (var c in Cards) c.IsSelected = c.Surah?.Id == cardVm?.Surah?.Id;
        if(cardVm?.Surah is null) return;
        var index = Cards.IndexOf(cardVm);
        if (index < 0) return;

        Dispatcher.UIThread.Post(() =>
        {
            var element = SurahRepeater.TryGetElement(index);
            element?.BringIntoView();
        }, DispatcherPriority.Loaded);
    }
}