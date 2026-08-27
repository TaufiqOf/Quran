using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Quran.Helpers;
using Quran.Models;
using Quran.Views.Component;

namespace Quran.Views.Pages;

public partial class HomeView : AView
{
    public List<CardComponent> Cards { get; set; } = new List<CardComponent>();
    private IEnumerable<Surah> _surahs = new List<Surah>();
    private List<SurahOrder> _surahOrder;
    private List<SurahSynopsis> _surahSynopsis;

    public HomeView()
    {
        InitializeComponent();
    }
    
    public override Task Load()
    {
        _surahs = GetData.GetSurahs();
        _surahOrder = GetData.SurahOrder();
        _surahSynopsis = GetData.SurahSynopsis();
        foreach (var surah in _surahs)
        {
           var synopsis = _surahSynopsis.FirstOrDefault(q => q.SurahId == surah.Id);
            var card = new CardComponent(surah,synopsis);
            Cards.Add(card);
            ItemsControl.Items.Add(card);
        }
        return Task.CompletedTask;
    }
}