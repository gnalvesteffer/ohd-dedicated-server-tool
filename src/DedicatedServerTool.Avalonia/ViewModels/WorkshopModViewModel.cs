using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class WorkshopModViewModel : ObservableObject
{
    private readonly ServerProfile _serverProfile;
    private readonly Action<long> _onDelete;

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

    private Bitmap? _modCoverImage;
    public Bitmap? ModCoverImage
    {
        get => _modCoverImage;
        set => SetProperty(ref _modCoverImage, value);
    }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            var isEnabled = false;
            if (value)
            {
                isEnabled = InstalledWorkshopModUtility.EnableMod(_serverProfile.InstallDirectory, WorkshopId);
            }
            else
            {
                isEnabled = InstalledWorkshopModUtility.DisableMod(_serverProfile.InstallDirectory, WorkshopId);
            }
            SetProperty(ref _isEnabled, isEnabled);
        }
    }

    public ICommand OpenWorkshopPageCommand { get; }
    public ICommand DeleteModCommand { get; }
    public ICommand LoadWorkshopCoverImageCommand { get; }

    public WorkshopModViewModel(ServerProfile serverProfile, long workshopId, Action<long> onDelete)
    {
        _serverProfile = serverProfile;
        _workshopId = workshopId;
        _modName = InstalledWorkshopModUtility.GetModName(_serverProfile.InstallDirectory, WorkshopId);
        _isEnabled = InstalledWorkshopModUtility.IsModEnabled(_serverProfile.InstallDirectory, WorkshopId);
        _onDelete = onDelete;
        OpenWorkshopPageCommand = new RelayCommand(OpenWorkshopPage);
        DeleteModCommand = new AsyncRelayCommand(RemoveWorkshopIdAsync);
        LoadWorkshopCoverImageCommand = new AsyncRelayCommand(LoadWorkshopCoverImage);
    }

    private void OpenWorkshopPage()
    {
        InstalledWorkshopModUtility.OpenWorkshopPage(WorkshopId);
    }

    private async Task RemoveWorkshopIdAsync()
    {
        await SteamCmdUtility.UnsubscribeFromModAsync(_serverProfile.InstallDirectory, WorkshopId);
        _onDelete(WorkshopId);
    }

    private async Task LoadWorkshopCoverImage()
    {
        var imagePath = await InstalledWorkshopModUtility.GetCoverImagePathAsync(_serverProfile.InstallDirectory, WorkshopId);
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }
        try
        {
            ModCoverImage = new Bitmap(imagePath);
        }
        catch
        {
            ModCoverImage = null;
        }
    }
}
