using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class MapBrowserViewModel : ObservableObject
{
    private readonly ServerProfile _profile;

    private ObservableCollection<MapViewModel> _maps = new();
    public ObservableCollection<MapViewModel> Maps
    {
        get => _maps;
        set => SetProperty(ref _maps, value);
    }

    private int _totalPakFiles = new();
    public int TotalPakFiles
    {
        get => _totalPakFiles;
        set => SetProperty(ref _totalPakFiles, value);
    }

    private int _totalProcessedPakFiles = new();
    public int TotalProcessedPakFiles
    {
        get => _totalProcessedPakFiles;
        set => SetProperty(ref _totalProcessedPakFiles, value);
    }

    private bool _isLoading = new();
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand FindMapsCommand { get; }

    public MapBrowserViewModel(ServerProfile profile)
    {
        _profile = profile;
        FindMapsCommand = new AsyncRelayCommand(FindMapsAsync);
    }

    private async Task FindMapsAsync()
    {
        IsLoading = true;
        var pakFilePaths = Directory.GetFiles(Path.Combine(_profile.InstallDirectory, "HarshDoorstop/Mods/"), "*.pak", SearchOption.AllDirectories);
        TotalPakFiles = pakFilePaths.Count();
        TotalProcessedPakFiles = 0;
        Maps.Clear();
        await MapNameFinderUtility.FindMapNamesInPaksAsync(pakFilePaths, (pakFilePath, mapNames) =>
        {
            ++TotalProcessedPakFiles;
            foreach (var mapName in mapNames)
            {
                Maps.Add(new(Path.GetFileName(pakFilePath), mapName));
            }
        });
        IsLoading = false;
    }
}
