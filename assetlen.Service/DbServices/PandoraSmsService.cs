using Microsoft.Extensions.Configuration;
using Refit;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.RefitApiCalls;

namespace assetlen.Service.DbServices
{
    public class PandoraSmsService : IPandoraSmsService
    {
        private readonly IPandoraSmsApi _api;
        private readonly string _username;
        private readonly string _password;
        private readonly string _sender;

        public PandoraSmsService(IConfiguration configuration)
        {
            // Load configuration from appsettings.json
            _username = configuration["PandoraSms:Username"];
            _password = configuration["PandoraSms:Password"];
            _sender = configuration["PandoraSms:Sender"];

            // Initialize Refit client
            _api = RestService.For<IPandoraSmsApi>("https://www.sms.thepandoranetworks.com");
        }

        public async Task<PandoraSmsResponse> SendSmsAsync(string number, string message)
        {
            try
            {
                var response = await _api.SendSmsAsync(
                    number: number,
                    message: message,
                    sender: _sender,
                    username: _username,
                    password: _password,
                    // Lower case, and it matters. The gateway rejects "Info" —
                    // and reports that rejection as "Invalid recipients", so the
                    // failure reads as a bad phone number and sends you into the
                    // number-cleaning code. The accepted values are "info" and
                    // the category "bulk"; everything else answers 405.
                    messageType: "info",
                    messageCategory: "bulk"
                );

                // The gateway answers HTTP 200 for business failures too, so the
                // status code says nothing; only the body does.
                return response.Content ?? new PandoraSmsResponse
                {
                    Success = false,
                    Messages = { $"Gateway returned {(int)response.StatusCode} with no body" }
                };
            }
            catch (ApiException ex)
            {
                return new PandoraSmsResponse { Success = false, Messages = { ex.Message } };
            }
            catch (Exception ex)
            {
                return new PandoraSmsResponse { Success = false, Messages = { $"Error sending SMS: {ex.Message}" } };
            }
        }
    }
}
