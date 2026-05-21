using BeaconLib;
using assetlen.Shared.Models.Models.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Newtonsoft.Json;
using Syncfusion.Blazor.Diagram;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace assetlen.Services
{

    public class ServiceDiscoverer : IDisposable
    {
        private readonly Probe _probe;
        private string? _apiBaseUrl;
        private List<string> _connectionLogs = new List<string>();
        public ServiceDiscoverer()
        {
            _probe = new Probe("BilltrickV2ServiceDiscovery");
            _probe.BeaconsUpdated += beacons => UpdateApiUrl(beacons);
            _probe.Start();
        }

        private async Task UpdateApiUrl(IEnumerable<BeaconLocation> beacons)
        {
            if (!string.IsNullOrEmpty(_apiBaseUrl))
            {
                return;
            }

            var beacon = beacons.FirstOrDefault();
            if (beacon != null)
            {
                // Write port to AppData
                var appDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BillTrickV2"
                );


                Directory.CreateDirectory(appDataDir);

                var endpoints = JsonConvert.DeserializeObject<List<DiscoveryMessageDto>>(beacon.Data);
                if (endpoints != null && endpoints.Any())
                {
                    foreach (var endpoint in endpoints)
                    {

                        try
                        {

                            using (var client = new HttpClient())
                            {
                                try
                                {

                                    _connectionLogs.Add($"Probing endpoint http://{endpoint.IpAddress}:{endpoint.Port}/api/authorization/login");
                                    client.Timeout = TimeSpan.FromSeconds(90);
                                    var request = new HttpRequestMessage(HttpMethod.Head, $"http://{endpoint.IpAddress}:{endpoint.Port}/api/authorization/login");
                                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")); // Add Accept header  
                                    var response = await client.SendAsync(request);
                                    if ((int)response.StatusCode == 405)
                                    {
                                        _apiBaseUrl = $"http://{endpoint.IpAddress}:{endpoint.Port}";
                                        break;
                                    }
                                    else if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
                                    {
                                        _apiBaseUrl = $"https://{endpoint.IpAddress}:{endpoint.Port}";
                                        break;
                                    }
                                }
                                catch (HttpRequestException ex)
                                {
                                    // Handle specific HTTP request exceptions if needed
                                    Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                                    _connectionLogs.Add($"HTTP Request Exception: {ex.Message}");
                                }
                                catch (Exception ex)
                                {
                                    // Handle timeout or cancellation
                                    Console.WriteLine($"Task Canceled Exception: {ex.Message}");
                                    _connectionLogs.Add($"Task Canceled Exception: {ex.Message}");
                                }
                            }

                        }
                        catch (HttpRequestException)
                        {
                            // Log or handle the exception as needed
                            _connectionLogs.Add($"HTTP Request Exception when probing endpoint http://{endpoint.IpAddress}:{endpoint.Port}/api/authorization/login");
                        }
                        catch (TaskCanceledException)
                        {
                            // Handle timeout or cancellation
                            _connectionLogs.Add($"Task Canceled Exception when probing endpoint http://{endpoint.IpAddress}:{endpoint.Port}/api/authorization/login");
                        }
                    }

                    File.WriteAllText(Path.Combine(appDataDir, "startup-logs.txt"), string.Join("\n", _connectionLogs));
                }
                else
                {
                    File.WriteAllText(Path.Combine(appDataDir, "startup-logs.txt"), "No endpoints found in beacon data.");
                }
            }
        }
        public async Task CheckApiAvailability()
        {

        }
        public string? GetDiscoveredApiUrl() => _apiBaseUrl;

        public void Dispose() => _probe.Stop();


    }
}
