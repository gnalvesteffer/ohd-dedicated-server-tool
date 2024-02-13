using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.Views;
using System;
using System.Diagnostics;
using System.Linq;
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
    public ICommand StartRconToolCommand { get; }

    public ServerProfileDetailsViewModel(ServerProfile profile)
    {
        _serverProfile = profile;
        OpenInstallDirectoryCommand = new RelayCommand(OpenInstallDirectory);
        EditServerProfileCommand = new RelayCommand(EditServerProfile);
        UpdateServerAndModsCommand = new AsyncRelayCommand(UpdateServerAndModsAsync);
        StartServerCommand = new AsyncRelayCommand(StartServerAsync);
        StartRconToolCommand = new RelayCommand(StartRconTool);
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

    private async Task UpdateServerAndModsAsync()
    {
        var failedWorkshopIds = await ServerUtility.UpdateServerAndModsAsync(ServerProfile);
        if (failedWorkshopIds.Any())
        {
            MessageBoxWindow.Show("Error", $"An unexpected issue occurred while updating. Consider retrying, SteamCMD can be unreliable sometimes. Failed Workshop ItemIDs: {string.Join(", ", failedWorkshopIds)}");
        }
    }

    private async Task StartServerAsync()
    {
        try
        {
            await ServerUtility.StartServerAsync(ServerProfile);
        }
        catch (Exception exception)
        {
            MessageBoxWindow.Show("Error", exception.Message);
        }
    }

    private void StartRconTool()
    {
        if (!ServerProfile.RconPort.HasValue || ServerProfile.RconPassword == null)
        {
            MessageBoxWindow.Show("Error", "Cannot start RCON tool. RCON Port or RCON Password is not configured.");
        }
        VehlawRconUtility.Start("127.0.0.1", ServerProfile.RconPort!.Value, ServerProfile.RconPassword!);
    }
}
