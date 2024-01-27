using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class ServerProfileSetupViewModel : ObservableObject
{
    private readonly Action _onSave;

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

    private ObservableCollection<long> _installedWorkshopIds = new();
    public ObservableCollection<long> InstalledWorkshopIds
    {
        get => _installedWorkshopIds;
        set => SetProperty(ref _installedWorkshopIds, value);
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

    private ObservableCollection<IpAddressInfo> _localIpAddressInfos = new();
    public ObservableCollection<IpAddressInfo> LocalIpAddressInfos
    {
        get => _localIpAddressInfos;
        set => SetProperty(ref _localIpAddressInfos, value);
    }

    private IpAddressInfo? _selectedMultihomeIpAddressInfo;
    public IpAddressInfo? SelectedMultihomeIpAddressInfo
    {
        get => _selectedMultihomeIpAddressInfo;
        set
        {
            ServerProfile.MultihomeIp = value?.IpAddress.ToString();
            SetProperty(ref _selectedMultihomeIpAddressInfo, value);
        }
    }

    public ICommand SelectInstallDirectoryCommand { get; }
    public ICommand SaveServerProfileCommand { get; }
    public ICommand DiscardServerProfileSetupCommand { get; }
    public ICommand DownloadServerFilesCommand { get; }
    public ICommand OpenAssetScannerCommand { get; }
    public ICommand OpenAssetListCommand { get; }
    public ICommand AddWorkshopIdCommand { get; }
    public ICommand RefreshInstalledMods { get; }

    public ServerProfileSetupViewModel(ServerProfile serverProfile, Action onDiscardServerProfile, Action onSave)
    {
        _serverProfile = serverProfile;
        _serverProfile.PropertyChanged += _serverProfile_PropertyChanged;
        _onSave = onSave;
        SelectInstallDirectoryCommand = new AsyncRelayCommand(SelectInstallDirectoryAsync);
        SaveServerProfileCommand = new RelayCommand(SaveServerProfile);
        DiscardServerProfileSetupCommand = new RelayCommand(onDiscardServerProfile);
        DownloadServerFilesCommand = new AsyncRelayCommand(DownloadOrUpdateServerFilesAsync);
        OpenAssetScannerCommand = new RelayCommand(OpenAssetScanner);
        OpenAssetListCommand = new RelayCommand(OpenAssetList);
        AddWorkshopIdCommand = new AsyncRelayCommand(AddWorkshopIdAsync);
        RefreshInstalledMods = new RelayCommand(RehydrateInstalledWorkshopIds);
        HydrateLocalIpAddresses();
        UpdateServerFileProperties();
        RehydrateInstalledWorkshopIds();
        UpdateCanSave();
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
            RehydrateInstalledWorkshopIds();
        }
        UpdateCanSave();
    }

    private void HydrateLocalIpAddresses()
    {
        _localIpAddressInfos = new(IpUtility.GetLocalIPv4Addresses());
        _selectedMultihomeIpAddressInfo = _localIpAddressInfos.FirstOrDefault(ipInfo => ipInfo.IpAddress.ToString().Equals(ServerProfile.MultihomeIp, StringComparison.OrdinalIgnoreCase));
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
            ServerProfile.MaxPlayers.HasValue &&
            (!ServerProfile.IsRconEnabled || !string.IsNullOrWhiteSpace(ServerProfile.RconPassword));
    }

    private void SaveServerProfile()
    {
        if (!CanSave)
        {
            return;
        }
        ServerProfile.IsSetUp = true;
        ServerUtility.WriteIniAndConfigFiles(ServerProfile);
        _onSave();
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
        await SteamCmdUtility.DownloadOrUpdateDedicatedServerAsync(ServerProfile.InstallDirectory);
        IsDownloadingServerFiles = false;
        UpdateServerFileProperties();
    }

    private void OpenAssetScanner()
    {
        var scanWindow = new PakScanWindow(ServerProfile);
        scanWindow.Show();
    }

    private void OpenAssetList()
    {
        var assetListWindow = new AssetListWindow();
        assetListWindow.Show();
    }

    private async Task AddWorkshopIdAsync()
    {
        if (!EnteredWorkshopId.HasValue)
        {
            return;
        }

        RehydrateInstalledWorkshopIds();
        if (InstalledWorkshopIds.Contains(EnteredWorkshopId.Value))
        {
            EnteredWorkshopId = null;
            return;
        }

        var workshopId = EnteredWorkshopId.Value;
        EnteredWorkshopId = null;

        if (!await SteamCmdUtility.DownloadOrUpdateModAsync(ServerProfile.InstallDirectory, workshopId))
        {
            MessageBoxWindow.Show("Error", "An unexpected issue occurred while downloading mod. Consider retrying, SteamCMD can be unreliable sometimes.");
        }
        RehydrateInstalledWorkshopIds();
    }

    private void RehydrateInstalledWorkshopIds()
    {
        InstalledWorkshopIds = new(ServerProfile.GetInstalledWorkshopIds());
    }
}
