using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace assetlen.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductDetailFeedbackController : ControllerBase
    {
        private readonly IProductDetailFeedbackDAL _feedbackDAL;

        public ProductDetailFeedbackController(IProductDetailFeedbackDAL feedbackDAL)
        {
            _feedbackDAL = feedbackDAL;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        private string GetUserName() => User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "";
        private string? GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);
        private bool IsAdmin() => User.IsInRole(UserRoles.Contractor) || User.IsInRole(UserRoles.AssetlenSuperAdmin) || User.IsInRole(UserRoles.Contractor);

        #region Feedback CRUD

        /// <summary>
        /// Create new feedback on a document fragment
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Crew},{UserRoles.AssetlenSuperAdmin}")]
        [ProducesResponseType(typeof(ProductDetailFeedbackDto), 200)]
        public async Task<ActionResult> CreateFeedback([FromBody] ProductDetailFeedbackCreateDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.CreateFeedback(
                dto,
                GetUserId(),
                GetUserName(),
                GetUserEmail(),
                cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get feedback by ID
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProductDetailFeedbackDto), 200)]
        public async Task<ActionResult> GetFeedbackById([FromQuery][Required] string id, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetFeedbackById(id, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Query feedback with filters and pagination
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PaginationDetails<ProductDetailFeedbackDto>), 200)]
        public async Task<ActionResult> GetFeedback([FromBody] ProductDetailFeedbackQueryDto query, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetFeedback(query, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get all feedback for a product/book
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDetailFeedbackDto>), 200)]
        public async Task<ActionResult> GetFeedbackByProductId([FromQuery][Required] string productId, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetFeedbackByProductId(productId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get all feedback for a specific section (ProductDetail)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDetailFeedbackDto>), 200)]
        public async Task<ActionResult> GetFeedbackByProductDetailId([FromQuery][Required] string productDetailId, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetFeedbackByProductDetailId(productDetailId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get feedback for a specific fragment
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDetailFeedbackDto>), 200)]
        public async Task<ActionResult> GetFeedbackByFragment(
            [FromQuery][Required] string productDetailId,
            [FromQuery][Required] string fragmentId,
            CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetFeedbackByFragment(productDetailId, fragmentId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get current user's feedback
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDetailFeedbackDto>), 200)]
        public async Task<ActionResult> GetMyFeedback(CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetMyFeedback(GetUserId(), cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Delete feedback (owner or admin only)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteFeedback([FromQuery][Required] string id, CancellationToken cancellationToken = default)
        {
            // Verify ownership or admin
            var feedbackResult = await _feedbackDAL.GetFeedbackById(id, cancellationToken);
            if (!feedbackResult.IsSuccess)
                return StatusCode(feedbackResult.StatusCode, feedbackResult.Error);

            if (feedbackResult.Data.SuggestedByUserId != GetUserId() && !IsAdmin())
                return Forbid();

            var result = await _feedbackDAL.DeleteFeedback(id, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        #endregion

        #region Admin Actions

        /// <summary>
        /// Update feedback status (admin only)
        /// </summary>
        [HttpPut]
        [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Contractor},{UserRoles.AssetlenSuperAdmin}")]
        [ProducesResponseType(typeof(ProductDetailFeedbackDto), 200)]
        public async Task<ActionResult> UpdateFeedbackStatus([FromBody] ProductDetailFeedbackUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.UpdateFeedbackStatus(dto, GetUserId(), cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Apply a suggested edit to the document (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Contractor},{UserRoles.AssetlenSuperAdmin}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> ApplySuggestedEdit([FromQuery][Required] string feedbackId, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.ApplySuggestedEdit(feedbackId, GetUserId(), cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        #endregion

        #region Replies

        /// <summary>
        /// Add a reply to feedback
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Crew},{UserRoles.Contractor},{UserRoles.AssetlenSuperAdmin}")]
        [ProducesResponseType(typeof(ProductDetailFeedbackReplyDto), 200)]
        public async Task<ActionResult> CreateReply([FromBody] ProductDetailFeedbackReplyCreateDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.CreateReply(
                dto,
                GetUserId(),
                GetUserName(),
                IsAdmin(),
                cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get replies for a feedback item
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDetailFeedbackReplyDto>), 200)]
        public async Task<ActionResult> GetReplies([FromQuery][Required] string feedbackId, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetRepliesByFeedbackId(feedbackId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Delete a reply (owner or admin only)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> DeleteReply([FromQuery][Required] string replyId, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.DeleteReply(replyId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        #endregion

        #region Approval Workflow

        /// <summary>
        /// Initiate an approval for a suggested edit (admin only)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(FeedbackApprovalDto), 200)]
        [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Contractor},{UserRoles.AssetlenSuperAdmin}")]
        public async Task<ActionResult> InitiateApproval([FromBody] FeedbackApprovalCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (!IsAdmin())
                return Forbid();

            var result = await _feedbackDAL.InitiateApproval(
                dto,
                GetUserId(),
                GetUserName(),
                cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get all approvals for a feedback item
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<FeedbackApprovalDto>), 200)]
        public async Task<ActionResult> GetApprovals([FromQuery][Required] string feedbackId, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetApprovalsByFeedbackId(feedbackId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        /// <summary>
        /// Check if current user has already approved a feedback item
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult> HasUserApproved([FromQuery][Required] string feedbackId, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.HasUserApproved(feedbackId, GetUserId(), cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get feedback statistics
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(FeedbackStatsDto), 200)]
        public async Task<ActionResult> GetFeedbackStats([FromQuery] string? productId = null, CancellationToken cancellationToken = default)
        {
            var result = await _feedbackDAL.GetFeedbackStats(productId, cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        #endregion
    }
}
