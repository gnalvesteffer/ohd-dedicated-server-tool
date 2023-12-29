using Avalonia.Controls;
using Avalonia.Interactivity;
using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.ViewModels;

namespace DedicatedServerTool.Avalonia.Views;

public partial class MapBrowserWindow : Window
{
    public MapBrowserWindow(ServerProfile serverProfile)
    {
        InitializeComponent();
        DataContext = new MapBrowserViewModel(serverProfile);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is MapBrowserViewModel viewModel)
        {
            viewModel.FindMapsCommand.Execute(null);
        }
    }
}