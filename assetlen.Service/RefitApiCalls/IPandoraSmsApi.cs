using Refit;
using assetlen.Service.DbServices.ServiceInterfaces;

namespace assetlen.Service.RefitApiCalls
{
    public interface IPandoraSmsApi
    {
        [Get("/API/send_sms/")]
        Task<ApiResponse<PandoraSmsResponse>> SendSmsAsync(
            [AliasAs("number")] string number,
            [AliasAs("message")] string message,
            [AliasAs("sender")] string sender,
            [AliasAs("username")] string username,
            [AliasAs("password")] string password,
            [AliasAs("message_type")] string messageType,
            [AliasAs("message_category")] string messageCategory
        );
    }
}
