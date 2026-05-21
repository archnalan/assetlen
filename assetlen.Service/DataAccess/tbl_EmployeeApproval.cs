using assetlen.Shared.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace assetlen.Service.DataAccess
{
    /// <summary>
    /// Tracks individual admin approvals for promoting a general user to employee status.
    /// Two or more approvals are required before IsEmployee is set to true on the AppUser.
    /// </summary>
    public class tbl_EmployeeApproval : BaseEntity
    {
        /// <summary>
        /// The user being considered for promotion to employee
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string TargetUserId { get; set; } = string.Empty;

        /// <summary>
        /// The admin user who submitted this approval
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string ApproverUserId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the approver (cached for history)
        /// </summary>
        [MaxLength(100)]
        public string? ApproverUserName { get; set; }

        /// <summary>
        /// True if approving the promotion, false if rejecting
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Optional comment from the approver
        /// </summary>
        [MaxLength(500)]
        public string? Comment { get; set; }

        /// <summary>
        /// When this approval was submitted
        /// </summary>
        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation to the target user
        /// </summary>
        [ForeignKey("TargetUserId")]
        public virtual AppUser? TargetUser { get; set; }
    }
}
