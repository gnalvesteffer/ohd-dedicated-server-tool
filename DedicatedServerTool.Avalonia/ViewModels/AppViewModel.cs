using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using DedicatedServerTool.Avalonia.Models;
using System.Diagnostics;

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
                    ServerProfileSetupViewModel = new(SelectedServerProfile, DeleteServerProfileSetupCommand);
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
    public ICommand EditServerProfileCommand { get; }
    public ICommand DeleteServerProfileSetupCommand { get; }
    public ICommand StartServerCommand { get; }
    public ICommand SaveAppStateCommand { get; }

    public AppViewModel()
    {
        _appState = AppStateRepository.Load();
        ServerProfiles = new ObservableCollection<ServerProfile>(_appState.ServerProfiles);
        OpenInstallDirectoryCommand = new RelayCommand(OpenInstallDirectory);
        CreateServerProfileCommand = new RelayCommand(CreateServerProfile);
        EditServerProfileCommand = new RelayCommand(EditServerProfile);
        DeleteServerProfileSetupCommand = new RelayCommand(DeleteServerProfileSetup);
        StartServerCommand = new RelayCommand(StartServer);
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
            ServerName = "My OHD Server",
            Port = 7777,
            RconPort = 7779,
            QueryPort = 27005,
            MinPlayers = 1,
            MaxPlayers = 16,
            BluforBotCount = 0,
            OpforBotCount = 0,
            IsAutoBalanceEnabled = true,
            InitialMapName = "Risala",
            MapCycleText = "Argonne\nMontecassino\nLamDong\nKhafji_P\nRisala",
        };

        ServerProfiles.Add(serverProfile);
        SelectedServerProfile = serverProfile;
    }

    private void EditServerProfile()
    {
        if (SelectedServerProfile == null)
        {
            return;
        }
        SelectedServerProfile.IsSetUp = false;
    }

    private void DeleteServerProfileSetup()
    {
        if (SelectedServerProfile == null)
        {
            return;
        }

        ServerProfiles.Remove(SelectedServerProfile);
        SelectedServerProfile = null;
    }

    private void StartServer()
    {
        if (SelectedServerProfile == null)
        {
            return;
        }

        ServerUtility.StartServer(SelectedServerProfile);
    }

    private void SaveAppState()
    {
        AppStateRepository.Save(_appState);
    }
}
