using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
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

    private bool? _areServerFilesInstalled;
    public bool? AreServerFilesInstalled
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

    public ICommand SelectInstallDirectoryCommand { get; }
    public ICommand SubmitServerProfileSetupCommand { get; }
    public ICommand DiscardServerProfileSetupCommand { get; }
    public ICommand DownloadServerFilesCommand { get; }

    public ServerProfileSetupViewModel(ServerProfile serverProfile, ICommand onSubmitServerProfile, ICommand onDiscardServerProfile)
    {
        _serverProfile = serverProfile;
        _serverProfile.PropertyChanged += _serverProfile_PropertyChanged;
        SelectInstallDirectoryCommand = new AsyncRelayCommand(SelectInstallDirectoryAsync);
        SubmitServerProfileSetupCommand = onSubmitServerProfile;
        DiscardServerProfileSetupCommand = onDiscardServerProfile;
        DownloadServerFilesCommand = new AsyncRelayCommand(DownloadServerFilesAsync);
        UpdateServerFileProperties();
    }

    private void _serverProfile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerProfile.InstallDirectory))
        {
            if (!Directory.Exists(ServerProfile.InstallDirectory))
            {
                AreServerFilesInstalled = null;
                CanDownloadServerFiles = false;
                return;
            }
            UpdateServerFileProperties();
        }
    }

    private void UpdateServerFileProperties()
    {
        AreServerFilesInstalled = File.Exists(Path.Combine(ServerProfile.InstallDirectory ?? string.Empty, @"HarshDoorstop\Binaries\Win64\HarshDoorstopServer-Win64-Shipping.exe"));
        CanDownloadServerFiles = AreServerFilesInstalled == false;
    }

    private async Task SelectInstallDirectoryAsync()
    {
        if (TopLevel == null)
        {
            return;
        }

        var result = await TopLevel.StorageProvider.OpenFolderPickerAsync(new() { Title = "Select server install directory..." });
        var installDirectory = result.FirstOrDefault()?.Path.LocalPath;
        ServerProfile.InstallDirectory = installDirectory ?? string.Empty;
    }

    private async Task DownloadServerFilesAsync()
    {
        IsDownloadingServerFiles = true;
        var isSuccess = await SteamCmdUtility.DownloadDedicatedServerAsync(ServerProfile.InstallDirectory);
        IsDownloadingServerFiles = false;
    }
}
