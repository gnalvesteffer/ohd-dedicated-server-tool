using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using DedicatedServerTool.Avalonia.Models;

namespace DedicatedServerTool.Avalonia.Core;
internal static class IpUtility
{
    public static IEnumerable<IpAddressInfo> GetLocalIPv4Addresses()
    {
        var networkInterfaceAndIpAddressPairs = new List<IpAddressInfo>
        {
            new(null, IPAddress.Any)
        };
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    networkInterfaceAndIpAddressPairs.Add(new(networkInterface, ip.Address));
                }
            }
        }
        return networkInterfaceAndIpAddressPairs;
    }
}
