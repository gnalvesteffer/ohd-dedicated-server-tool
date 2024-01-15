using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Core;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using DedicatedServerTool.Avalonia.Models;
using System.Reflection;
using System;

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
            if (ServerProfileDetailsViewModel != null)
            {
                ServerProfileDetailsViewModel.TopLevel = value;
            }
            if (ServerProfileSetupViewModel != null)
            {
                ServerProfileSetupViewModel.TopLevel = value;
            }
        }
    }

    public string AppTitle => $"OHD Dedicated Server Tool v{Assembly.GetEntryAssembly()!.GetName().Version} by Xorberax";

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
                    ServerProfileSetupViewModel = new(SelectedServerProfile, () => DeleteServerProfileSetupCommand.Execute(null), () => SaveAppStateCommand.Execute(null));
                    ServerProfileSetupViewModel.TopLevel = TopLevel;
                    ServerProfileDetailsViewModel = new(SelectedServerProfile);
                    ServerProfileDetailsViewModel.TopLevel = TopLevel;
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

    private ServerProfileDetailsViewModel? _serverProfileDetailsViewModel;
    public ServerProfileDetailsViewModel? ServerProfileDetailsViewModel
    {
        get => _serverProfileDetailsViewModel;
        set => SetProperty(ref _serverProfileDetailsViewModel, value);
    }

    public ICommand CreateServerProfileCommand { get; }
    public ICommand DeleteServerProfileSetupCommand { get; }
    public ICommand SaveAppStateCommand { get; }
    public ICommand OpenPayPalLinkCommand { get; }
    public ICommand OpenPatreonLinkCommand { get; }

    public AppViewModel()
    {
        _appState = AppStateRepository.Load();
        ServerProfiles = new ObservableCollection<ServerProfile>(_appState.ServerProfiles);
        CreateServerProfileCommand = new RelayCommand(CreateServerProfile);
        DeleteServerProfileSetupCommand = new RelayCommand(DeleteServerProfileSetup);
        SaveAppStateCommand = new RelayCommand(SaveAppState);
        OpenPayPalLinkCommand = new RelayCommand(() => UrlUtility.OpenUrl("https://www.paypal.com/donate/?cmd=_s-xclick&hosted_button_id=923HVE4MUDRSA&source=url"));
        OpenPatreonLinkCommand = new RelayCommand(() => UrlUtility.OpenUrl("https://www.patreon.com/xorberax"));
        SelectedServerProfile = ServerProfiles.FirstOrDefault();
    }

    private void CreateServerProfile()
    {
        var serverProfile = new ServerProfile();
        ServerProfiles.Add(serverProfile);
        SelectedServerProfile = serverProfile;
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

    private void SaveAppState()
    {
        AppStateRepository.Save(_appState);
    }
}
