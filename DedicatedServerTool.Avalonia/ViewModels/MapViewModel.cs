using CommunityToolkit.Mvvm.ComponentModel;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class MapViewModel : ObservableObject
{
    private string _pakFilePath = string.Empty;
    public string PakFilePath
    {
        get => _pakFilePath;
        set => SetProperty(ref _pakFilePath, value);
    }

    private string _mapName = string.Empty;
    public string MapName
    {
        get => _mapName;
        set => SetProperty(ref _mapName, value);
    }

    public MapViewModel(string pakFilePath, string mapName)
    {
        PakFilePath = pakFilePath;
        MapName = mapName;
    }
}
