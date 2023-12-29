using CommunityToolkit.Mvvm.ComponentModel;

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

    private int _maxPlayers;
    public int MaxPlayers
    {
        get => _maxPlayers;
        set => SetProperty(ref _maxPlayers, value);
    }

    private int _port;
    public int Port
    {
        get => _port;
        set
        {
            if (SetProperty(ref _port, value))
            {
                ClientPort = value + 1;
            }
        }
    }

    private int _clientPort;
    public int ClientPort
    {
        get => _clientPort;
        set => SetProperty(ref _clientPort, value);
    }

    private int _queryPort;
    public int QueryPort
    {
        get => _queryPort;
        set => SetProperty(ref _queryPort, value);
    }

    private int _rconPort;
    public int RconPort
    {
        get => _rconPort;
        set => SetProperty(ref _rconPort, value);
    }
}
