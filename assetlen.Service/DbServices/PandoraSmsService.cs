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
                    messageType: "Info",
                    messageCategory: "bulk"
                );

                return response.Content;
            }
            catch (ApiException ex)
            {
                // Handle API errors
                return new PandoraSmsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                // Handle general errors
                return new PandoraSmsResponse
                {
                    Success = false,
                    ErrorMessage = $"Error sending SMS: {ex.Message}"
                };
            }
        }
    }
}
