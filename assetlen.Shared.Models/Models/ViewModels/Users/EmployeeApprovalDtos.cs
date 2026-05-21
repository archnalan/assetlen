using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
    /// <summary>
    /// Submitted by an admin when approving/rejecting a user's promotion to employee
    /// </summary>
    public class SubmitEmployeeApprovalDto
    {
        /// <summary>
        /// The user being promoted
        /// </summary>
        [Required]
        public string TargetUserId { get; set; } = string.Empty;

        /// <summary>
        /// True = approve, False = reject
        /// </summary>
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// Optional comment
        /// </summary>
        [MaxLength(500)]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Response showing current approval state for a user's employee promotion
    /// </summary>
    public class EmployeeApprovalStatusDto
    {
        public string TargetUserId { get; set; } = string.Empty;
        public string TargetUserName { get; set; } = string.Empty;
        public int ApprovalCount { get; set; }
        public int RejectionCount { get; set; }
        public bool IsPromoted { get; set; }
        public bool CurrentAdminHasVoted { get; set; }
        public List<EmployeeApprovalEntryDto> Approvals { get; set; } = new();
    }

    public class EmployeeApprovalEntryDto
    {
        public string Id { get; set; } = string.Empty;
        public string ApproverUserId { get; set; } = string.Empty;
        public string? ApproverUserName { get; set; }
        public bool IsApproved { get; set; }
        public string? Comment { get; set; }
        public DateTime ApprovedAt { get; set; }
    }
}
