using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.ViewModels;
using System;

namespace DedicatedServerTool.Avalonia.Views;

public partial class WorkshopMod : UserControl
{
    public static readonly StyledProperty<long> WorkshopIdProperty = AvaloniaProperty.Register<WorkshopMod, long>(nameof(WorkshopId));
    public static readonly StyledProperty<ServerProfile> ServerProfileProperty = AvaloniaProperty.Register<WorkshopMod, ServerProfile>(nameof(ServerProfile));

    public long WorkshopId
    {
        get => GetValue(WorkshopIdProperty);
        set => SetValue(WorkshopIdProperty, value);
    }

    public ServerProfile ServerProfile
    {
        get => GetValue(ServerProfileProperty);
        set => SetValue(ServerProfileProperty, value);
    }

    public WorkshopMod()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Tag is ServerProfileSetupViewModel serverProfileSetupViewModel && DataContext is not WorkshopModViewModel)
        {
            DataContext = new WorkshopModViewModel(serverProfileSetupViewModel.ServerProfile, WorkshopId);
        }
    }
}