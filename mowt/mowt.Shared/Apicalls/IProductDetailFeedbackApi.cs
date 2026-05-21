using mowt.Shared.Models.Models.ViewModels;
using Refit;
using System.ComponentModel.DataAnnotations;

namespace mowt.Shared.Apicalls
{
    public interface IProductDetailFeedbackApi
    {
        #region Feedback CRUD

        [Post("/api/ProductDetailFeedback/CreateFeedback")]
        Task<IApiResponse<ProductDetailFeedbackDto>> CreateFeedback(
            [Body] ProductDetailFeedbackCreateDto dto,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/GetFeedbackById")]
        Task<IApiResponse<ProductDetailFeedbackDto>> GetFeedbackById(
            [Query][Required] string id,
            CancellationToken cancellationToken = default);

        [Post("/api/ProductDetailFeedback/GetFeedback")]
        Task<IApiResponse<PaginationDetails<ProductDetailFeedbackDto>>> GetFeedback(
            [Body] ProductDetailFeedbackQueryDto query,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/GetFeedbackByProductId")]
        Task<IApiResponse<List<ProductDetailFeedbackDto>>> GetFeedbackByProductId(
            [Query][Required] string productId,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/GetFeedbackByProductDetailId")]
        Task<IApiResponse<List<ProductDetailFeedbackDto>>> GetFeedbackByProductDetailId(
            [Query][Required] string productDetailId,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/GetFeedbackByFragment")]
        Task<IApiResponse<List<ProductDetailFeedbackDto>>> GetFeedbackByFragment(
            [Query][Required] string productDetailId,
            [Query][Required] string fragmentId,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/GetMyFeedback")]
        Task<IApiResponse<List<ProductDetailFeedbackDto>>> GetMyFeedback(
            CancellationToken cancellationToken = default);

        [Delete("/api/ProductDetailFeedback/DeleteFeedback")]
        Task<IApiResponse<bool>> DeleteFeedback(
            [Query][Required] string id,
            CancellationToken cancellationToken = default);

        #endregion

        #region Admin Actions

        [Put("/api/ProductDetailFeedback/UpdateFeedbackStatus")]
        Task<IApiResponse<ProductDetailFeedbackDto>> UpdateFeedbackStatus(
            [Body] ProductDetailFeedbackUpdateDto dto,
            CancellationToken cancellationToken = default);

        [Post("/api/ProductDetailFeedback/ApplySuggestedEdit")]
        Task<IApiResponse<bool>> ApplySuggestedEdit(
            [Query][Required] string feedbackId,
            CancellationToken cancellationToken = default);

        #endregion

        #region Replies

        [Post("/api/ProductDetailFeedback/CreateReply")]
        Task<IApiResponse<ProductDetailFeedbackReplyDto>> CreateReply(
            [Body] ProductDetailFeedbackReplyCreateDto dto,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/GetReplies")]
        Task<IApiResponse<List<ProductDetailFeedbackReplyDto>>> GetReplies(
            [Query][Required] string feedbackId,
            CancellationToken cancellationToken = default);

        [Delete("/api/ProductDetailFeedback/DeleteReply")]
        Task<IApiResponse<bool>> DeleteReply(
            [Query][Required] string replyId,
            CancellationToken cancellationToken = default);

        #endregion

        #region Approval Workflow

        [Post("/api/ProductDetailFeedback/InitiateApproval")]
        Task<IApiResponse<FeedbackApprovalDto>> InitiateApproval(
            [Body] FeedbackApprovalCreateDto dto,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/GetApprovals")]
        Task<IApiResponse<List<FeedbackApprovalDto>>> GetApprovals(
            [Query][Required] string feedbackId,
            CancellationToken cancellationToken = default);

        [Get("/api/ProductDetailFeedback/HasUserApproved")]
        Task<IApiResponse<bool>> HasUserApproved(
            [Query][Required] string feedbackId,
            CancellationToken cancellationToken = default);

        #endregion

        #region Statistics

        [Get("/api/ProductDetailFeedback/GetFeedbackStats")]
        Task<IApiResponse<FeedbackStatsDto>> GetFeedbackStats(
            [Query] string? productId = null,
            CancellationToken cancellationToken = default);

        #endregion
    }
}
