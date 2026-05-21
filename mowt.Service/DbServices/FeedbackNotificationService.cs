using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using mowt.Web.Controllers.SmtpClient;
using Hangfire;

namespace mowt.Service.DbServices
{
    /// <summary>
    /// Handles real-time SignalR notifications and email notifications for feedback
    /// </summary>
    public class FeedbackNotificationService : IFeedbackNotificationService
    {
        private readonly IHubContext<FeedbackHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FeedbackNotificationService> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public FeedbackNotificationService(
            IHubContext<FeedbackHub> hubContext,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<FeedbackNotificationService> logger,
            IBackgroundJobClient backgroundJobClient)
        {
            _hubContext = hubContext;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task NotifyFeedbackCreated(ProductDetailFeedbackDto feedback)
        {
            try
            {
                var eventDto = new FeedbackRealtimeEventDto
                {
                    EventType = FeedbackEventType.FeedbackCreated,
                    FeedbackId = feedback.Id!,
                    ProductId = feedback.ProductId,
                    ProductDetailId = feedback.ProductDetailId,
                    FragmentId = feedback.FragmentId,
                    TriggeredByUserId = feedback.SuggestedByUserId,
                    TriggeredByUserName = feedback.SuggestedByUserName,
                    Timestamp = DateTime.UtcNow
                };

                // Notify admins watching this product
                await _hubContext.Clients
                    .Group($"product-{feedback.ProductId}-admins")
                    .SendAsync("FeedbackEvent", eventDto);

                // Notify users watching this section
                await _hubContext.Clients
                    .Group($"section-{feedback.ProductDetailId}")
                    .SendAsync("FeedbackEvent", eventDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending feedback created notification");
            }
        }

        public async Task NotifyStatusChanged(
            ProductDetailFeedbackDto feedback,
            FeedbackStatus oldStatus,
            FeedbackStatus newStatus)
        {
            try
            {
                var eventDto = new FeedbackRealtimeEventDto
                {
                    EventType = FeedbackEventType.StatusChanged,
                    FeedbackId = feedback.Id!,
                    ProductId = feedback.ProductId,
                    ProductDetailId = feedback.ProductDetailId,
                    FragmentId = feedback.FragmentId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Timestamp = DateTime.UtcNow
                };

                // Notify the user who submitted the feedback
                if (!string.IsNullOrEmpty(feedback.SuggestedByUserId))
                {
                    await _hubContext.Clients
                        .User(feedback.SuggestedByUserId)
                        .SendAsync("FeedbackEvent", eventDto);
                }

                // Send email notification (background job)
                if (!string.IsNullOrEmpty(feedback.SuggestedByUserEmail))
                {
                    _backgroundJobClient.Enqueue(() => SendStatusChangeEmail(
                        feedback.SuggestedByUserEmail,
                        feedback.SuggestedByUserName ?? "Reader",
                        feedback.ProductName ?? "Unknown Book",
                        feedback.SectionTitle ?? "Unknown Section",
                        oldStatus.ToString(),
                        newStatus.ToString(),
                        feedback.ProductId,
                        feedback.Id!));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status changed notification");
            }
        }

        public async Task NotifyNewReply(
            tbl_ProductDetailFeedback feedback,
            ProductDetailFeedbackReplyDto reply,
            bool isAdminReply)
        {
            try
            {
                var eventDto = new FeedbackRealtimeEventDto
                {
                    EventType = FeedbackEventType.NewReply,
                    FeedbackId = feedback.Id,
                    ProductId = feedback.ProductId,
                    ProductDetailId = feedback.ProductDetailId,
                    FragmentId = feedback.FragmentId,
                    NewReply = reply,
                    TriggeredByUserId = reply.UserId,
                    TriggeredByUserName = reply.UserName,
                    Timestamp = DateTime.UtcNow
                };

                // Notify the feedback author if this is an admin reply
                if (isAdminReply && !string.IsNullOrEmpty(feedback.SuggestedByUserId))
                {
                    await _hubContext.Clients
                        .User(feedback.SuggestedByUserId)
                        .SendAsync("FeedbackEvent", eventDto);

                    // Send email notification (background job)
                    if (!string.IsNullOrEmpty(feedback.SuggestedByUserEmail))
                    {
                        _backgroundJobClient.Enqueue(() => SendReplyNotificationEmail(
                            feedback.SuggestedByUserEmail,
                            feedback.SuggestedByUserName ?? "Reader",
                            reply.UserName ?? "Admin",
                            reply.Message,
                            feedback.ProductId,
                            feedback.Id));
                    }
                }
                else if (!isAdminReply)
                {
                    // Notify admins of user reply
                    await _hubContext.Clients
                        .Group($"product-{feedback.ProductId}-admins")
                        .SendAsync("FeedbackEvent", eventDto);
                }

                // Notify everyone watching this feedback
                await _hubContext.Clients
                    .Group($"feedback-{feedback.Id}")
                    .SendAsync("FeedbackEvent", eventDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new reply notification");
            }
        }

        public async Task NotifyEditApplied(ProductDetailFeedbackDto feedback)
        {
            try
            {
                var eventDto = new FeedbackRealtimeEventDto
                {
                    EventType = FeedbackEventType.StatusChanged,
                    FeedbackId = feedback.Id!,
                    ProductId = feedback.ProductId,
                    ProductDetailId = feedback.ProductDetailId,
                    FragmentId = feedback.FragmentId,
                    NewStatus = FeedbackStatus.Approved,
                    Timestamp = DateTime.UtcNow
                };

                // Notify the user who submitted the feedback
                if (!string.IsNullOrEmpty(feedback.SuggestedByUserId))
                {
                    await _hubContext.Clients
                        .User(feedback.SuggestedByUserId)
                        .SendAsync("FeedbackEvent", eventDto);
                }

                // Send email notification (background job)
                if (!string.IsNullOrEmpty(feedback.SuggestedByUserEmail))
                {
                    _backgroundJobClient.Enqueue(() => SendEditAppliedEmail(
                        feedback.SuggestedByUserEmail,
                        feedback.SuggestedByUserName ?? "Reader",
                        feedback.ProductName ?? "Unknown Book",
                        feedback.SectionTitle ?? "Unknown Section",
                        feedback.ProductId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending edit applied notification");
            }
        }

        #region Email Methods (called via Hangfire)

        public void SendStatusChangeEmail(
            string toEmail,
            string userName,
            string bookName,
            string sectionTitle,
            string oldStatus,
            string newStatus,
            string productId,
            string feedbackId)
        {
            try
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://app.example.com";
                var readUrl = $"{baseUrl}/books/{productId}/read?feedbackId={feedbackId}";

                var emailDto = new EmailDto
                {
                    SmtpHost = _configuration["SmtpSettings:Host"],
                    SmtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587"),
                    FromEmail = _configuration["SmtpSettings:SenderEmail"],
                    SenderName = _configuration["SmtpSettings:SenderName"],
                    emailSenderAccount = _configuration["SmtpSettings:Username"],
                    emailSenderSecret = _configuration["SmtpSettings:Password"],
                    ReplyToEmail = _configuration["SmtpSettings:ReplyToEmail"],
                    CompanyName = _configuration["SmtpSettings:CompanyName"],
                    websiteLink = _configuration["SmtpSettings:WebsiteLink"],
                    ToEmail = toEmail,
                    Subject = $"Your feedback status has been updated - {bookName}",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2>Hello {userName},</h2>
                            <p>Your feedback on <strong>{bookName}</strong> has been updated.</p>
                            <div style='background: #f5f5f5; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                                <p><strong>Section:</strong> {sectionTitle}</p>
                                <p><strong>Previous Status:</strong> {oldStatus}</p>
                                <p><strong>New Status:</strong> {newStatus}</p>
                            </div>
                            <p>
                                <a href='{readUrl}' style='background: #0066cc; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                                    View Feedback
                                </a>
                            </p>
                            <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                                This is an automated notification. Please do not reply to this email.
                            </p>
                        </body>
                        </html>"
                };

                _emailService.SendEmail(emailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status change email to {Email}", toEmail);
            }
        }

        public void SendReplyNotificationEmail(
            string toEmail,
            string userName,
            string replierName,
            string replyPreview,
            string productId,
            string feedbackId)
        {
            try
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://app.example.com";
                var readUrl = $"{baseUrl}/books/{productId}/read?feedbackId={feedbackId}";
                var previewText = replyPreview.Length > 200
                    ? replyPreview.Substring(0, 200) + "..."
                    : replyPreview;

                var emailDto = new EmailDto
                {
                    SmtpHost = _configuration["SmtpSettings:Host"],
                    SmtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587"),
                    FromEmail = _configuration["SmtpSettings:SenderEmail"],
                    SenderName = _configuration["SmtpSettings:SenderName"],
                    emailSenderAccount = _configuration["SmtpSettings:Username"],
                    emailSenderSecret = _configuration["SmtpSettings:Password"],
                    ReplyToEmail = _configuration["SmtpSettings:ReplyToEmail"],
                    CompanyName = _configuration["SmtpSettings:CompanyName"],
                    websiteLink = _configuration["SmtpSettings:WebsiteLink"],
                    ToEmail = toEmail,
                    Subject = $"{replierName} replied to your feedback",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2>Hello {userName},</h2>
                            <p><strong>{replierName}</strong> has replied to your feedback:</p>
                            <div style='background: #f5f5f5; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #0066cc;'>
                                <p style='margin: 0;'>{previewText}</p>
                            </div>
                            <p>
                                <a href='{readUrl}' style='background: #0066cc; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                                    View Conversation
                                </a>
                            </p>
                            <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                                This is an automated notification. Please do not reply to this email.
                            </p>
                        </body>
                        </html>"
                };

                _emailService.SendEmail(emailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reply notification email to {Email}", toEmail);
            }
        }

        public void SendEditAppliedEmail(
            string toEmail,
            string userName,
            string bookName,
            string sectionTitle,
            string productId)
        {
            try
            {
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://app.example.com";
                var readUrl = $"{baseUrl}/books/{productId}/read";

                var emailDto = new EmailDto
                {
                    SmtpHost = _configuration["SmtpSettings:Host"],
                    SmtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587"),
                    FromEmail = _configuration["SmtpSettings:SenderEmail"],
                    SenderName = _configuration["SmtpSettings:SenderName"],
                    emailSenderAccount = _configuration["SmtpSettings:Username"],
                    emailSenderSecret = _configuration["SmtpSettings:Password"],
                    ReplyToEmail = _configuration["SmtpSettings:ReplyToEmail"],
                    CompanyName = _configuration["SmtpSettings:CompanyName"],
                    websiteLink = _configuration["SmtpSettings:WebsiteLink"],
                    ToEmail = toEmail,
                    Subject = $"Your suggested edit has been applied - {bookName}",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2>Congratulations {userName}!</h2>
                            <p>Your suggested edit for <strong>{bookName}</strong> has been reviewed and applied.</p>
                            <div style='background: #e8f5e9; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #4caf50;'>
                                <p><strong>Section:</strong> {sectionTitle}</p>
                                <p>Your contribution is now part of the document. Thank you for helping improve the content!</p>
                            </div>
                            <p>
                                <a href='{readUrl}' style='background: #4caf50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                                    View Updated Content
                                </a>
                            </p>
                            <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                                This is an automated notification. Please do not reply to this email.
                            </p>
                        </body>
                        </html>"
                };

                _emailService.SendEmail(emailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending edit applied email to {Email}", toEmail);
            }
        }

        #endregion
    }
}
