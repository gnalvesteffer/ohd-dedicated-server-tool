using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class ServerProfileDetailsViewModel : ObservableObject
{
    public TopLevel? TopLevel;

    private ServerProfile _serverProfile;
    public ServerProfile ServerProfile
    {
        get => _serverProfile;
        set => SetProperty(ref _serverProfile, value);
    }

    public ICommand OpenInstallDirectoryCommand { get; }
    public ICommand EditServerProfileCommand { get; }
    public ICommand UpdateServerAndModsCommand { get; }
    public ICommand StartServerCommand { get; }

    public ServerProfileDetailsViewModel(ServerProfile profile)
    {
        _serverProfile = profile;
        OpenInstallDirectoryCommand = new RelayCommand(OpenInstallDirectory);
        EditServerProfileCommand = new RelayCommand(EditServerProfile);
        UpdateServerAndModsCommand = new AsyncRelayCommand(UpdateServerAndModsAsync);
        StartServerCommand = new RelayCommand(StartServer);
    }

    private void OpenInstallDirectory()
    {
        if (TopLevel == null)
        {
            return;
        }

        Process.Start("explorer", ServerProfile.InstallDirectory);
    }

    private void EditServerProfile()
    {
        ServerProfile.IsSetUp = false;
    }

    private Task UpdateServerAndModsAsync()
    {
        var updateServerTask = SteamCmdUtility.DownloadOrUpdateDedicatedServerAsync(ServerProfile.InstallDirectory);
        var updateModsTask = Parallel.ForEachAsync(ServerProfile.GetInstalledWorkshopIds(), async (workshopId, cancellationToken) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            await SteamCmdUtility.DownloadOrUpdateModAsync(ServerProfile.InstallDirectory, workshopId);
        });
        return Task.WhenAll(updateServerTask, updateModsTask);
    }

    private void StartServer()
    {
        ServerUtility.StartServer(ServerProfile);
    }

}
