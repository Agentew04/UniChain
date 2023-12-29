using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P {
    public static class IpManager {
        private readonly static Logger logger = new(nameof(IpManager));

        public static IPAddress? GetPublicIp() {
            HttpClient client = new() {
                Timeout = TimeSpan.FromSeconds(2)
            };

            List<string> urls = [
                "https://icanhazip.com",
                "https://ipinfo.io/ip",
                "http://checkip.dyndns.org"
            ];

            try {
                var response = client.GetAsync(urls[0]).Result;
                string responseString = response.Content.ReadAsStringAsync().Result.Replace("\n",string.Empty);
                var address = IPAddress.Parse(responseString);
                logger.Log($"Public IP: {address}");
                return address;
            } catch (TaskCanceledException) {
                try {
                    var response = client.GetAsync(urls[1]).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result.Replace("\n", string.Empty);
                    var address = IPAddress.Parse(responseString);
                    logger.Log($"Public IP: {address}");
                    return address;
                } catch (TaskCanceledException) {
                    try {
                        var response = client.GetAsync(urls[2]).Result;
                        var responseString = response.Content.ReadAsStringAsync().Result.Replace("\n", string.Empty);
                        var address = IPAddress.Parse(responseString.Split(':')[1]);
                        logger.Log($"Public IP: {address}");
                        return address;
                    } catch (TaskCanceledException) {
                        logger.Log("Could not get public IP");
                        return null;
                    }
                }
            }
        }

        public static List<IPAddress> GetPrivateIpsAsync() {
            var ips = Dns.GetHostAddresses(Dns.GetHostName());
            logger.Log($"Private IPs: {string.Join(", ", ips.ToList())}");
            return ips.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
        }

        public static Address GetCurrentAddress(Guid nodeId, int port) {
            Address addr = new(nodeId, 
                GetPublicIp() ?? IPAddress.None,
                GetPrivateIpsAsync()[0],
                port);
            return addr;
        }
    }
}
