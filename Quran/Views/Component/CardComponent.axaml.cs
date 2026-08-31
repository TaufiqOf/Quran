using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Models;
namespace Quran.Views.Component;

public partial class CardComponent : UserControl
{
    public static event Action<Surah>? CardClick;

    public CardComponent()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SurahCardViewModel vm)
        {
            LoadData(vm.Surah, vm.Synopsis);
            IsSelected = vm.IsSelected;
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(SurahCardViewModel.IsSelected))
                {
                    IsSelected = vm.IsSelected;
                }
            };
        }
    }

    public bool IsSelected
    {
        get => CardBorder.Classes.Contains("selected");
        set
        {
            if (value)
                CardBorder.Classes.Add("selected");
            else
                CardBorder.Classes.Remove("selected");
        }
    }

    public void LoadData(Surah surah, SurahSynopsis? synopsis)
    {
        TextBlockTitle.Text = $"{surah.Id}. {surah.Transliteration}";
        TextBlockArabicTitle.Text = surah.Name;
        TextBlockSubtitle.Text = surah.Translation;
        TextBlockMetadata.Text = $"{surah.Type} - {surah.TotalVerses} verses";
        TextBlockTags.Text = string.Join(" - ", synopsis?.Themes ?? []);
        TextBlockDescription.Text = synopsis?.Synopsis;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SurahCardViewModel vm)
        {
            CardClick?.Invoke(vm.Surah);
        }
    }
}