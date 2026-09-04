using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Quran.Helpers;
using Quran.Models;

namespace Quran.Views.Pages;

public partial class AboutView : AView
{
    public AboutView()
    {
        InitializeComponent();
    }

    public override async Task Load(params object?[] parameter)
    {
        await Task.CompletedTask;
    }

    public override async Task Reload(params object?[] parameter)
    {
        await Task.CompletedTask;
    }
}