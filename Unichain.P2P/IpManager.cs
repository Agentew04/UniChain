using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P {
    public static class IpManager {
        public static async Task<List<string>> GetIpsAsync() {
            var ips = await Dns.GetHostAddressesAsync(Dns.GetHostName());
            return ips.Select(ip => ip.ToString()).ToList();
        }
    }
}
