using Avalonia.Controls;
using DedicatedServerTool.Avalonia.ViewModels;

namespace DedicatedServerTool.Avalonia.Views;

public partial class AssetListWindow : Window
{
    public AssetListWindow()
    {
        InitializeComponent();
        DataContext = new AssetListViewModel();
    }
}