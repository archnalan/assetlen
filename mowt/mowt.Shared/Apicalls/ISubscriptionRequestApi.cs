using mowt.Shared.Models.Models.ViewModels;
using Refit;
using System.ComponentModel.DataAnnotations;

namespace mowt.Shared.Apicalls
{
    public interface ISubscriptionRequestApi
    {
        #region Public

        [Post("/api/SubscriptionRequest/Submit")]
        Task<IApiResponse<SubscriptionRequestDto>> Submit(
            [Body] SubscriptionRequestCreateDto dto,
            CancellationToken cancellationToken = default);

        #endregion

        #region Admin – Read

        [Get("/api/SubscriptionRequest/GetById")]
        Task<IApiResponse<SubscriptionRequestDto>> GetById(
            [Query][Required] string id,
            CancellationToken cancellationToken = default);

        [Post("/api/SubscriptionRequest/GetAll")]
        Task<IApiResponse<List<SubscriptionRequestDto>>> GetAll(
            [Body] SubscriptionRequestQueryDto query,
            CancellationToken cancellationToken = default);

        [Get("/api/SubscriptionRequest/GetStats")]
        Task<IApiResponse<SubscriptionRequestStatsDto>> GetStats(
            CancellationToken cancellationToken = default);

        #endregion

        #region Admin – Workflow

        [Put("/api/SubscriptionRequest/IssueQuote")]
        Task<IApiResponse<SubscriptionRequestDto>> IssueQuote(
            [Body] SubscriptionRequestQuoteDto dto,
            CancellationToken cancellationToken = default);

        [Put("/api/SubscriptionRequest/ConfirmPayment")]
        Task<IApiResponse<SubscriptionRequestDto>> ConfirmPayment(
            [Body] SubscriptionRequestPaymentDto dto,
            CancellationToken cancellationToken = default);

        [Put("/api/SubscriptionRequest/UpdateStatus")]
        Task<IApiResponse<SubscriptionRequestDto>> UpdateStatus(
            [Body] SubscriptionRequestStatusUpdateDto dto,
            CancellationToken cancellationToken = default);

        #endregion

        #region Admin – Seats

        [Post("/api/SubscriptionRequest/AddSeat")]
        Task<IApiResponse<SubscriptionSeatDto>> AddSeat(
            [Body] SubscriptionSeatCreateDto dto,
            CancellationToken cancellationToken = default);

        [Post("/api/SubscriptionRequest/AddSeatsBulk")]
        Task<IApiResponse<List<SubscriptionSeatDto>>> AddSeatsBulk(
            [Body] SubscriptionSeatBulkCreateDto dto,
            CancellationToken cancellationToken = default);

        [Delete("/api/SubscriptionRequest/RemoveSeat")]
        Task<IApiResponse<bool>> RemoveSeat(
            [Query][Required] string seatId,
            CancellationToken cancellationToken = default);

        [Get("/api/SubscriptionRequest/GetSeats")]
        Task<IApiResponse<List<SubscriptionSeatDto>>> GetSeats(
            [Query][Required] string requestId,
            CancellationToken cancellationToken = default);

        #endregion
    }
}
