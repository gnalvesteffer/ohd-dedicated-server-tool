using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class ServerProfileSetupViewModel : ObservableObject
{
    public TopLevel? TopLevel;

    private ServerProfile _serverProfile;
    public ServerProfile ServerProfile
    {
        get => _serverProfile;
        set => SetProperty(ref _serverProfile, value);
    }

    private bool _areServerFilesInstalled;
    public bool AreServerFilesInstalled
    {
        get => _areServerFilesInstalled;
        set => SetProperty(ref _areServerFilesInstalled, value);
    }

    private bool _canDownloadServerFiles;
    public bool CanDownloadServerFiles
    {
        get => _canDownloadServerFiles;
        set => SetProperty(ref _canDownloadServerFiles, value);
    }

    private bool _isDownloadingServerFiles;
    public bool IsDownloadingServerFiles
    {
        get => _isDownloadingServerFiles;
        set => SetProperty(ref _isDownloadingServerFiles, value);
    }

    private long? _enteredWorkshopId;
    public long? EnteredWorkshopId
    {
        get => _enteredWorkshopId;
        set => SetProperty(ref _enteredWorkshopId, value);
    }

    private long? _selectedWorkshopId;
    public long? SelectedWorkshopId
    {
        get => _selectedWorkshopId;
        set => SetProperty(ref _selectedWorkshopId, value);
    }

    private bool _canSave;
    public bool CanSave
    {
        get => _canSave;
        set => SetProperty(ref _canSave, value);
    }

    public ICommand SelectInstallDirectoryCommand { get; }
    public ICommand SubmitServerProfileSetupCommand { get; }
    public ICommand DiscardServerProfileSetupCommand { get; }
    public ICommand DownloadServerFilesCommand { get; }
    public ICommand OpenAssetScannerCommand { get; }
    public ICommand AddWorkshopIdCommand { get; }
    public ICommand RemoveWorkshopIdCommand { get; }

    public ServerProfileSetupViewModel(ServerProfile serverProfile, ICommand onDiscardServerProfile)
    {
        _serverProfile = serverProfile;
        _serverProfile.PropertyChanged += _serverProfile_PropertyChanged;
        SelectInstallDirectoryCommand = new AsyncRelayCommand(SelectInstallDirectoryAsync);
        SubmitServerProfileSetupCommand = new RelayCommand(SubmitServerProfileSetup);
        DiscardServerProfileSetupCommand = onDiscardServerProfile;
        DownloadServerFilesCommand = new AsyncRelayCommand(DownloadOrUpdateServerFilesAsync);
        OpenAssetScannerCommand = new RelayCommand(OpenAssetScanner);
        AddWorkshopIdCommand = new AsyncRelayCommand(AddWorkshopIdAsync);
        RemoveWorkshopIdCommand = new AsyncRelayCommand(RemoveWorkshopIdAsync);
        UpdateServerFileProperties();
    }

    private void _serverProfile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerProfile.InstallDirectory))
        {
            if (!Directory.Exists(ServerProfile.InstallDirectory))
            {
                AreServerFilesInstalled = false;
                CanDownloadServerFiles = false;
                return;
            }
            UpdateServerFileProperties();
        }
        UpdateCanSave();
    }

    private void UpdateServerFileProperties()
    {
        AreServerFilesInstalled = File.Exists(Path.Combine(ServerProfile.InstallDirectory ?? string.Empty, @"HarshDoorstop\Binaries\Win64\HarshDoorstopServer-Win64-Shipping.exe"));
        CanDownloadServerFiles = Directory.Exists(ServerProfile.InstallDirectory) && !AreServerFilesInstalled;
        UpdateCanSave();
    }

    private void UpdateCanSave()
    {
        CanSave = 
            AreServerFilesInstalled && 
            !string.IsNullOrWhiteSpace(ServerProfile.ServerName) &&
            ServerProfile.Port.HasValue &&
            ServerProfile.ClientPort.HasValue &&
            ServerProfile.QueryPort.HasValue &&
            ServerProfile.RconPort.HasValue &&
            !string.IsNullOrWhiteSpace(ServerProfile.InitialMapName) &&
            ServerProfile.MaxPlayers.HasValue;
    }

    private void SubmitServerProfileSetup()
    {
        if (!CanSave)
        {
            return;
        }
        ServerProfile.IsSetUp = true;
    }

    private async Task SelectInstallDirectoryAsync()
    {
        if (TopLevel == null)
        {
            return;
        }

        var result = await TopLevel.StorageProvider.OpenFolderPickerAsync(new() { Title = "Select server install directory..." });
        if (!result.Any())
        {
            return;
        }
        var installDirectory = result.FirstOrDefault()?.Path.LocalPath;
        ServerProfile.InstallDirectory = installDirectory ?? string.Empty;
    }

    private async Task DownloadOrUpdateServerFilesAsync()
    {
        IsDownloadingServerFiles = true;
        var result = await SteamCmdUtility.DownloadOrUpdateDedicatedServerAsync(ServerProfile.InstallDirectory);
        IsDownloadingServerFiles = false;
        UpdateServerFileProperties();
    }

    private void OpenAssetScanner()
    {
        var scanWindow = new PakScanWindow(ServerProfile);
        scanWindow.Show();
    }

    private Task AddWorkshopIdAsync()
    {
        if (!EnteredWorkshopId.HasValue)
        {
            return Task.CompletedTask;
        }

        if (ServerProfile.WorkshopIds.Contains(EnteredWorkshopId.Value))
        {
            EnteredWorkshopId = null;
            return Task.CompletedTask;
        }

        var workshopId = EnteredWorkshopId.Value;
        ServerProfile.WorkshopIds.Add(workshopId);
        EnteredWorkshopId = null;

        return SteamCmdUtility.DownloadOrUpdateModAsync(ServerProfile.InstallDirectory, workshopId);
    }

    private Task RemoveWorkshopIdAsync()
    {
        if (!SelectedWorkshopId.HasValue)
        {
            return Task.CompletedTask;
        }

        var workshopId = SelectedWorkshopId.Value;
        ServerProfile.WorkshopIds.Remove(workshopId);

        return SteamCmdUtility.UnsubscribeFromModAsync(ServerProfile.InstallDirectory, workshopId);
    }
}
