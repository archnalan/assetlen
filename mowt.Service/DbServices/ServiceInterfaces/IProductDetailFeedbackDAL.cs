using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    public interface IProductDetailFeedbackDAL
    {
        // Feedback CRUD
        Task<ServiceResult<ProductDetailFeedbackDto>> CreateFeedback(ProductDetailFeedbackCreateDto dto, string userId, string userName, string? userEmail, CancellationToken cancellationToken = default);
        Task<ServiceResult<ProductDetailFeedbackDto>> GetFeedbackById(string id, CancellationToken cancellationToken = default);
        Task<ServiceResult<PaginationDetails<ProductDetailFeedbackDto>>> GetFeedback(ProductDetailFeedbackQueryDto query, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetFeedbackByProductId(string productId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetFeedbackByProductDetailId(string productDetailId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetFeedbackByFragment(string productDetailId, string fragmentId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ProductDetailFeedbackDto>>> GetMyFeedback(string userId, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> DeleteFeedback(string id, CancellationToken cancellationToken = default);

        // Admin actions
        Task<ServiceResult<ProductDetailFeedbackDto>> UpdateFeedbackStatus(ProductDetailFeedbackUpdateDto dto, string reviewerUserId, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> ApplySuggestedEdit(string feedbackId, string adminUserId, CancellationToken cancellationToken = default);

        // Two-step approval workflow
        Task<ServiceResult<FeedbackApprovalDto>> InitiateApproval(FeedbackApprovalCreateDto dto, string userId, string userName, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<FeedbackApprovalDto>>> GetApprovalsByFeedbackId(string feedbackId, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> HasUserApproved(string feedbackId, string userId, CancellationToken cancellationToken = default);

        // Replies
        Task<ServiceResult<ProductDetailFeedbackReplyDto>> CreateReply(ProductDetailFeedbackReplyCreateDto dto, string userId, string userName, bool isAdmin, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ProductDetailFeedbackReplyDto>>> GetRepliesByFeedbackId(string feedbackId, CancellationToken cancellationToken = default);
        Task<ServiceResult<bool>> DeleteReply(string replyId, CancellationToken cancellationToken = default);

        // Statistics
        Task<ServiceResult<FeedbackStatsDto>> GetFeedbackStats(string? productId = null, CancellationToken cancellationToken = default);
    }
}
