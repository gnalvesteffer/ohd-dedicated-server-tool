using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open.Nat;

public class PortForwardManager
{
    private readonly NatDiscoverer _discoverer = new NatDiscoverer();
    private readonly List<Mapping> _mappings = new();
    private NatDevice? _natDevice;

    public async Task OpenPortsAsync(params ushort[] ports)
    {
        _natDevice = await _discoverer.DiscoverDeviceAsync();
        if (_natDevice == null)
        {
            throw new Exception("Failed to set up port forwarding via uPnP.");
        }

        foreach (var port in ports)
        {
            // Open port on TCP
            var tcpMapping = new Mapping(Protocol.Tcp | Protocol.Udp, port, port);
            await _natDevice.CreatePortMapAsync(tcpMapping);
            _mappings.Add(tcpMapping);
        }
    }

    public async Task ClosePortsAsync()
    {
        if (_natDevice == null)
        {
            return;
        }

        foreach (var mapping in _mappings)
        {
            await _natDevice.DeletePortMapAsync(mapping);
        }
    }
}
