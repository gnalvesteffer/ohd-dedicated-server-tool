using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class WorkshopModViewModel : ObservableObject
{
    private readonly ServerProfile _serverProfile;

    private long _workshopId;
    public long WorkshopId
    {
        get => _workshopId;
        set => SetProperty(ref _workshopId, value);
    }

    private string _modName;
    public string ModName
    {
        get => _modName;
        set => SetProperty(ref _modName, value);
    }

    public WorkshopModViewModel(ServerProfile serverProfile, long workshopId)
    {
        _serverProfile = serverProfile;
        _workshopId = workshopId;
        _modName = InstalledWorkshopModUtility.GetModName(_serverProfile.InstallDirectory, WorkshopId);
    }
}
