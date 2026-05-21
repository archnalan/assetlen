using mowt.Shared.Models.Models.ViewModels;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IPandoraSmsService
    {
        Task<PandoraSmsResponse> SendSmsAsync(string number, string message);
    }

    public class PandoraSmsResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }
}
