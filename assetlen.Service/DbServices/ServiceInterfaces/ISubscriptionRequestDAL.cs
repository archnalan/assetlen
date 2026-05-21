using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface ISubscriptionRequestDAL
    {
        // Public submission
        Task<ServiceResult<SubscriptionRequestDto>> SubmitRequest(SubscriptionRequestCreateDto dto, string? userId, string? userName, string? userEmail, CancellationToken cancellationToken = default);

        // Read
        Task<ServiceResult<SubscriptionRequestDto>> GetById(string id, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<SubscriptionRequestDto>>> GetAll(SubscriptionRequestQueryDto query, CancellationToken cancellationToken = default);
        Task<ServiceResult<SubscriptionRequestStatsDto>> GetStats(CancellationToken cancellationToken = default);

        // Admin workflow
        Task<ServiceResult<SubscriptionRequestDto>> IssueQuote(SubscriptionRequestQuoteDto dto, string adminUserId, string adminUserName, CancellationToken cancellationToken = default);
        Task<ServiceResult<SubscriptionRequestDto>> ConfirmPayment(SubscriptionRequestPaymentDto dto, string adminUserId, CancellationToken cancellationToken = default);
        Task<ServiceResult<SubscriptionRequestDto>> UpdateStatus(SubscriptionRequestStatusUpdateDto dto, string adminUserId, CancellationToken cancellationToken = default);

        // Seat management
        Task<ServiceResult<SubscriptionSeatDto>> AddSeat(SubscriptionSeatCreateDto dto, string adminUserId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<SubscriptionSeatDto>>> AddSeatsBulk(SubscriptionSeatBulkCreateDto dto, string adminUserId, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> RemoveSeat(string seatId, string adminUserId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<SubscriptionSeatDto>>> GetSeatsByRequestId(string requestId, CancellationToken cancellationToken = default);
    }
}
