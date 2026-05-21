using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.statics;
using mowt.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace mowt.API.Domain
{
    public class OnlineIdentityVerifier : IOnlineIdentityVerifier
    {
        private IConfiguration _configuration;
        private ILogger<OnlineIdentityVerifier> _logger;


        public OnlineIdentityVerifier(IConfiguration configuration, ILogger<OnlineIdentityVerifier> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private const string PublicKey = """-----BEGIN CERTIFICATE-----MIIDfzCCAmegAwIBAgIUaj8dIy6LEIG9okZJm4EXZuKCbKAwDQYJKoZIhvcNAQELBQAwTzELMAkGA1UEBhMCVUcxEzARBgNVBAgMClNvbWUtU3RhdGUxEDAOBgNVBAcMB0thbXBhbGExGTAXBgNVBAoMEEJpbGx0cmlja0xpbWl0ZWQwHhcNMjUwNTIxMTkwMDQ2WhcNMzUwNTE5MTkwMDQ2WjBPMQswCQYDVQQGEwJVRzETMBEGA1UECAwKU29tZS1TdGF0ZTEQMA4GA1UEBwwHS2FtcGFsYTEZMBcGA1UECgwQQmlsbHRyaWNrTGltaXRlZDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAI44BPl6ek2wMJa1pFVrUf52PNaniF5hq+oiLKVzbmsu09t3gpLrdMwn8KCmrNImL5EUEYBVPvennpjIHN0DTUHoG56eP2HNCnvQZhFjQPByu++Kda6QujUkd8p1fJu0vmxHd3oGP5oM/74EHxb/ARc+ek6arHd87qTNUOUSIFzgd8WEY62kmQfXxO0DFc/ieiL2bBpKT8kXWhUNgxLB0dcSpYMAne2kCSqsaBQkafw2L1i4mI6x6NyKHNzbxK+ir+mdGRdtRfmz+4mq1QLItpLX7sBAsWJuXNde9hXoC+k8ganXTFBR/LhKzhqxSEaNLE73cO8qGKPVark0n6sfCEsCAwEAAaNTMFEwHQYDVR0OBBYEFG7ESrXRjn847N1hFLWAG2S+SVu+MB8GA1UdIwQYMBaAFG7ESrXRjn847N1hFLWAG2S+SVu+MA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQELBQADggEBAB+sJucIclvhwu2NsVArrk6m46BWOFqk9xyJZ3s1ErBht7akDNAe9EEDNg8H9y2MFiO2tqGOGFTtxjWRfi8LUyoKwFwOBGtMn/9OZAd+vgMvOxPENOnwRdlsR0hIbf52KMqTdALTQU2ZeD8t2n4bIup6oX1lIxUHidxabeX5mEVsu9GKZcolGHXEOMEWjTStbEU2+UrxmiSJ5ioTyxVjuQKuTy6xj1a7D+/ecY2VbhPTIQ5MPp1DFTv309SYvXEq50GP36+HzYUDpczha1rFsYiXgUGKHrrb9Q8N64Xme0nOiCNu2DB8+4OptWv6CCuvx/60CE5FUHlaxuYcas9VTQI=-----END CERTIFICATE-----""";

        public bool IsOnlineApi()
        {
            try
            {


                var privateKey = Environment.GetEnvironmentVariable("ONLINE_PRIVATE_KEY") ?? _configuration["ONLINE_PRIVATE_KEY"];
                if (!string.IsNullOrEmpty(privateKey))
                {
                    // Normalize PEM format by ensuring correct newlines
                    privateKey = privateKey.Trim();
                    if (!privateKey.StartsWith("-----BEGIN PRIVATE KEY-----") || !privateKey.EndsWith("-----END PRIVATE KEY-----"))
                    {
                        _logger.LogError("Invalid private key format");

                        return false;
                    }

                    // Extract base64 content and reconstruct with proper line breaks
                    var base64Content = privateKey
                        .Replace("-----BEGIN PRIVATE KEY-----", "")
                        .Replace("-----END PRIVATE KEY-----", "")
                        .Replace("\n", "")
                        .Trim();

                    var sb = new StringBuilder();
                    sb.AppendLine("-----BEGIN PRIVATE KEY-----");
                    for (int i = 0; i < base64Content.Length; i += 64)
                    {
                        int length = Math.Min(64, base64Content.Length - i);
                        sb.AppendLine(base64Content.Substring(i, length));
                    }
                    sb.AppendLine("-----END PRIVATE KEY-----");
                    privateKey = sb.ToString();
                }

                //_logger.LogInformation("Verifying online API with private key: {privateKey}", privateKey);

                if (string.IsNullOrEmpty(privateKey))
                {
                    //_logger.LogInformation("Private key is not set or empty. Local API running.");
                    return false;
                }

                // Create test signature
                var nonce = Guid.NewGuid().ToString();
                using var rsa = RSA.Create();
                rsa.ImportFromPem(privateKey);

                byte[] signature = rsa.SignData(
                    Encoding.UTF8.GetBytes(nonce),
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );

                // Verify using embedded public key
                var certPem = PublicKey;
                using var cert = X509Certificate2.CreateFromPem(certPem);
                using var rsaPub = cert.GetRSAPublicKey();

                var result = rsaPub.VerifyData(
                    Encoding.UTF8.GetBytes(nonce),
                    signature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );

                //if (result)
                //    _logger.LogInformation("Starting API as the live server: {result}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying online API");


                return false;
            }

        }

    }
}