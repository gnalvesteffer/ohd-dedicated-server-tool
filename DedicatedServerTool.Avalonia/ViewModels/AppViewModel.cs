using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using DedicatedServerTool.Avalonia.Models;
using System.Diagnostics;
using System.IO;

namespace DedicatedServerTool.Avalonia.ViewModels;

public partial class AppViewModel : ViewModelBase
{
    private readonly AppState _appState;

    private TopLevel? _topLevel;
    public TopLevel? TopLevel
    {
        get => _topLevel;
        set
        {
            _topLevel = value;
            if (ServerProfileSetupViewModel != null)
            {
                ServerProfileSetupViewModel.TopLevel = value;
            }
        }
    }

    public ObservableCollection<ServerProfile> ServerProfiles
    {
        get => _appState.ServerProfiles;
        set => _appState.ServerProfiles = value;
    }

    private ServerProfile? _selectedServerProfile;
    public ServerProfile? SelectedServerProfile
    {
        get => _selectedServerProfile;
        set
        {
            if (SetProperty(ref _selectedServerProfile, value))
            {
                IsServerProfileSelected = value != null;
                if (SelectedServerProfile != null)
                {
                    ServerProfileSetupViewModel = new(SelectedServerProfile, SubmitServerProfileSetupCommand, DiscardServerProfileSetupCommand);
                    ServerProfileSetupViewModel.TopLevel = TopLevel;
                }
            }
        }
    }

    private bool _isServerProfileSelected;
    public bool IsServerProfileSelected
    {
        get => _isServerProfileSelected;
        set => SetProperty(ref _isServerProfileSelected, value);
    }

    private bool _isSelectedServerProfileSetUp;
    public bool IsSelectedServerProfileSetUp
    {
        get => _isSelectedServerProfileSetUp;
        set => SetProperty(ref _isSelectedServerProfileSetUp, value);
    }

    private ServerProfileSetupViewModel? _serverProfileSetupViewModel;
    public ServerProfileSetupViewModel? ServerProfileSetupViewModel
    {
        get => _serverProfileSetupViewModel;
        set => SetProperty(ref _serverProfileSetupViewModel, value);
    }

    public ICommand OpenInstallDirectoryCommand { get; }
    public ICommand CreateServerProfileCommand { get; }
    public ICommand SubmitServerProfileSetupCommand { get; }
    public ICommand DiscardServerProfileSetupCommand { get; }
    public ICommand SaveAppStateCommand { get; }

    public AppViewModel()
    {
        _appState = AppStateRepository.Load();
        ServerProfiles = new ObservableCollection<ServerProfile>(_appState.ServerProfiles);
        OpenInstallDirectoryCommand = new RelayCommand(OpenInstallDirectory);
        CreateServerProfileCommand = new RelayCommand(CreateServerProfile);
        SubmitServerProfileSetupCommand = new RelayCommand(SubmitServerProfileSetup);
        DiscardServerProfileSetupCommand = new RelayCommand(DiscardServerProfileSetup);
        SaveAppStateCommand = new RelayCommand(SaveAppState);
        SelectedServerProfile = ServerProfiles.FirstOrDefault();
    }

    private void OpenInstallDirectory()
    {
        if (SelectedServerProfile == null || TopLevel == null)
        {
            return;
        }

        Process.Start("explorer", SelectedServerProfile.InstallDirectory);
    }

    private void CreateServerProfile()
    {
        var serverProfile = new ServerProfile
        {
            ServerName = "My OHD Server"
        };
        ServerProfiles.Add(serverProfile);
        SelectedServerProfile = serverProfile;
    }

    private void SubmitServerProfileSetup()
    {
        if (SelectedServerProfile == null)
        {
            return;
        }

        bool HasName() => !string.IsNullOrWhiteSpace(SelectedServerProfile.ServerName);
        bool AreServerFilesInstalled() => File.Exists(Path.Combine(SelectedServerProfile.InstallDirectory ?? string.Empty, @"HarshDoorstop\Binaries\Win64\HarshDoorstopServer-Win64-Shipping.exe"));

        SelectedServerProfile.IsSetUp = HasName() && AreServerFilesInstalled();
    }

    private void DiscardServerProfileSetup()
    {
        if (SelectedServerProfile == null)
        {
            return;
        }

        ServerProfiles.Remove(SelectedServerProfile);
        SelectedServerProfile = null;
    }

    private void SaveAppState()
    {
        AppStateRepository.Save(_appState);
    }
}
