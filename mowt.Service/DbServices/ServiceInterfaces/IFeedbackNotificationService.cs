using mowt.Service.DataAccess;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;

namespace mowt.Service.DbServices.ServiceInterfaces
{
    /// <summary>
    /// Service for handling feedback notifications (real-time + email)
    /// </summary>
    public interface IFeedbackNotificationService
    {
        /// <summary>
        /// Notify when new feedback is created
        /// </summary>
        Task NotifyFeedbackCreated(ProductDetailFeedbackDto feedback);

        /// <summary>
        /// Notify when feedback status changes
        /// </summary>
        Task NotifyStatusChanged(ProductDetailFeedbackDto feedback, FeedbackStatus oldStatus, FeedbackStatus newStatus);

        /// <summary>
        /// Notify when a new reply is added
        /// </summary>
        Task NotifyNewReply(tbl_ProductDetailFeedback feedback, ProductDetailFeedbackReplyDto reply, bool isAdminReply);

        /// <summary>
        /// Notify when a suggested edit is applied
        /// </summary>
        Task NotifyEditApplied(ProductDetailFeedbackDto feedback);
    }
}
