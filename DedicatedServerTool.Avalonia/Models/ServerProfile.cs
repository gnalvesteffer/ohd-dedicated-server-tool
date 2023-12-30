using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DedicatedServerTool.Avalonia.Models;
public class ServerProfile : ObservableObject
{
    private bool _isSetUp;
    public bool IsSetUp
    {
        get => _isSetUp;
        set => SetProperty(ref _isSetUp, value);
    }

    private string _serverName = string.Empty;
    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    private string _installDirectory = string.Empty;
    public string InstallDirectory
    {
        get => _installDirectory;
        set => SetProperty(ref _installDirectory, value);
    }

    private string _initialMapName = string.Empty;
    public string InitialMapName
    {
        get => _initialMapName;
        set => SetProperty(ref _initialMapName, value);
    }

    private string _gameModePath = string.Empty;
    public string GameModePath
    {
        get => _gameModePath;
        set => SetProperty(ref _gameModePath, value);
    }

    private int? _minPlayers;
    public int? MinPlayers
    {
        get => _minPlayers;
        set => SetProperty(ref _minPlayers, value);
    }

    private int? _maxPlayers;
    public int? MaxPlayers
    {
        get => _maxPlayers;
        set => SetProperty(ref _maxPlayers, value);
    }

    private int? _bluforBotCount;
    public int? BluforBotCount
    {
        get => _bluforBotCount;
        set
        {
            SetProperty(ref _bluforBotCount, value);
        }
    }

    private int? _opforBotCount;
    public int? OpforBotCount
    {
        get => _opforBotCount;
        set
        {
            SetProperty(ref _opforBotCount, value);
        }
    }

    private int? _bluforTickets;
    public int? BluforTickets
    {
        get => _bluforTickets;
        set
        {
            SetProperty(ref _bluforTickets, value);
        }
    }

    private int? _opforTickets;
    public int? OpforTickets
    {
        get => _opforTickets;
        set
        {
            SetProperty(ref _opforTickets, value);
        }
    }

    private string _bluforFactionName = string.Empty;
    public string BluforFactionName
    {
        get => _bluforFactionName;
        set => SetProperty(ref _bluforFactionName, value);
    }

    private string _opforFactionName = string.Empty;
    public string OpforFactionName
    {
        get => _opforFactionName;
        set => SetProperty(ref _opforFactionName, value);
    }

    private int? _port;
    public int? Port
    {
        get => _port;
        set
        {
            if (SetProperty(ref _port, value))
            {
                ClientPort = value.HasValue ? value + 1 : null;
            }
        }
    }

    private int? _clientPort;
    public int? ClientPort
    {
        get => _clientPort;
        set => SetProperty(ref _clientPort, value);
    }

    private int? _queryPort;
    public int? QueryPort
    {
        get => _queryPort;
        set => SetProperty(ref _queryPort, value);
    }

    private int? _rconPort;
    public int? RconPort
    {
        get => _rconPort;
        set => SetProperty(ref _rconPort, value);
    }

    private string _serverPassword = string.Empty;
    public string ServerPassword
    {
        get => _serverPassword;
        set => SetProperty(ref _serverPassword, value);
    }

    private int? _teamIdToAutoAssignHumans;
    public int? TeamIdToAutoAssignHumans
    {
        get => _teamIdToAutoAssignHumans;
        set => SetProperty(ref _teamIdToAutoAssignHumans, value);
    }

    private bool _isBotAutoFillEnabled;
    public bool IsBotAutoFillEnabled
    {
        get => _isBotAutoFillEnabled;
        set => SetProperty(ref _isBotAutoFillEnabled, value);
    }

    private bool _shouldDisableKitRestrictions;
    public bool ShouldDisableKitRestrictions
    {
        get => _shouldDisableKitRestrictions;
        set => SetProperty(ref _shouldDisableKitRestrictions, value);
    }

    private string _mapCycleText = string.Empty;
    public string MapCycleText
    {
        get => _mapCycleText;
        set => SetProperty(ref _mapCycleText, value);
    }

    private int? _respawnDurationSeconds;
    public int? RespawnDurationSeconds
    {
        get => _respawnDurationSeconds;
        set => SetProperty(ref _respawnDurationSeconds, value);
    }

    private bool _isFriendlyFireEnabled;
    public bool IsFriendlyFireEnabled
    {
        get => _isFriendlyFireEnabled;
        set => SetProperty(ref _isFriendlyFireEnabled, value);
    }
}
