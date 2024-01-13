using CommunityToolkit.Mvvm.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;

namespace DedicatedServerTool.Avalonia.Models;
public class IpAddressInfo : ObservableObject
{
    private NetworkInterface? _networkInterface;
    public NetworkInterface? NetworkInterface
    {
        get => _networkInterface;
        set => SetProperty(ref _networkInterface, value);
    }

    private IPAddress _ipAddress;
    public IPAddress IpAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    public IpAddressInfo(NetworkInterface? networkInterface, IPAddress ipAddress)
    {
        _networkInterface = networkInterface;
        _ipAddress = ipAddress;
    }
}
