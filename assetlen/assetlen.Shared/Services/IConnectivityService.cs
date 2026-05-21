using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Services
{
    public interface IConnectivityService
    {
        bool HasInternet { get; }
        string? ConnectionProfile { get; }   // e.g. "WiFi", "Ethernet"
    }
}
