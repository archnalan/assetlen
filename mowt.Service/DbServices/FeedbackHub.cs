using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace mowt.Service.DbServices
{
    /// <summary>
    /// SignalR Hub for real-time feedback notifications
    /// Allows anonymous for real-time updates - actual data operations are protected by API authorization
    /// </summary>
    [AllowAnonymous]
    public class FeedbackHub : Hub
    {
        private readonly ILogger<FeedbackHub> _logger;

        public FeedbackHub(ILogger<FeedbackHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Join a group to receive updates for a specific product/book
        /// </summary>
        public async Task JoinProductGroup(string productId, bool isAdmin = false)
        {
            var groupName = isAdmin
                ? $"product-{productId}-admins"
                : $"product-{productId}";

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("User {UserId} joined group {GroupName}", Context.UserIdentifier, groupName);
        }

        /// <summary>
        /// Leave a product group
        /// </summary>
        public async Task LeaveProductGroup(string productId, bool isAdmin = false)
        {
            var groupName = isAdmin
                ? $"product-{productId}-admins"
                : $"product-{productId}";

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("User {UserId} left group {GroupName}", Context.UserIdentifier, groupName);
        }

        /// <summary>
        /// Join a group to receive updates for a specific section
        /// </summary>
        public async Task JoinSectionGroup(string sectionId)
        {
            var groupName = $"section-{sectionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("User {UserId} joined group {GroupName}", Context.UserIdentifier, groupName);
        }

        /// <summary>
        /// Leave a section group
        /// </summary>
        public async Task LeaveSectionGroup(string sectionId)
        {
            var groupName = $"section-{sectionId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("User {UserId} left group {GroupName}", Context.UserIdentifier, groupName);
        }

        /// <summary>
        /// Join a group to receive updates for a specific feedback item
        /// </summary>
        public async Task JoinFeedbackGroup(string feedbackId)
        {
            var groupName = $"feedback-{feedbackId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("User {UserId} joined group {GroupName}", Context.UserIdentifier, groupName);
        }

        /// <summary>
        /// Leave a feedback group
        /// </summary>
        public async Task LeaveFeedbackGroup(string feedbackId)
        {
            var groupName = $"feedback-{feedbackId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogDebug("User {UserId} left group {GroupName}", Context.UserIdentifier, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("User {UserId} connected to FeedbackHub", Context.UserIdentifier);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User {UserId} disconnected from FeedbackHub", Context.UserIdentifier);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
