using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.Domain.Interfaces
{

    public class DiscoveryService
    {

        private readonly CancellationTokenSource _cts = new();
        private readonly string _serviceName = "mowtServiceDiscovery";
        private readonly int _servicePort;

        public DiscoveryService(int servicePort)
        {
            _servicePort = servicePort;
        }

        public async Task<string> CreateBroadcastMessage()
        {

            try
            {
                // Get all active LAN IPs
                var lanIps = GetLocalNetworkIPs();

                var output = new List<DiscoveryMessageDto>();
                foreach (var ip in lanIps)
                {
                    var message = new DiscoveryMessageDto
                    {
                        ServiceName = _serviceName,
                        IpAddress = ip.ToString(),
                        Port = _servicePort
                    };

                    output.Add(message);

                }

                var jsonMessage = JsonSerializer.Serialize(output);
                return jsonMessage;
            }
            catch { /* Ignore errors */ }


            return null;
        }

        private List<IPAddress> GetLocalNetworkIPs()
        {
            var ips = new List<IPAddress>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Skip non-operational or loopback interfaces
                if (ni.OperationalStatus != OperationalStatus.Up ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                {
                    // Get IPv4 addresses in private ranges
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        IsPrivateIP(addr.Address))
                    {
                        ips.Add(addr.Address);
                    }
                }
            }
            return ips;
        }

        private static bool IsPrivateIP(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            return bytes[0] switch
            {
                10 => true,                                      // 10.0.0.0/8
                172 when bytes[1] >= 16 && bytes[1] <= 31 => true, // 172.16.0.0/12
                192 when bytes[1] == 168 => true,                // 192.168.0.0/16
                _ => false
            };
        }



    }
}
