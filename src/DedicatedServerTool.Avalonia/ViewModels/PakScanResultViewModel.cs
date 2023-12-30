using CommunityToolkit.Mvvm.ComponentModel;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class PakScanResultViewModel : ObservableObject
{
    private string _pakFilePath = string.Empty;
    public string PakFilePath
    {
        get => _pakFilePath;
        set => SetProperty(ref _pakFilePath, value);
    }

    private string _result = string.Empty;
    public string Result
    {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    public PakScanResultViewModel(string pakFilePath, string result)
    {
        PakFilePath = pakFilePath;
        Result = result;
    }
}
