using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace mowt.Shared.Models.Models
{
    public class AppUser : IdentityUser, IBaseEntity
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string? Address { get; set; }
        public string? Aboutme { get; set; }
        public string? Industry { get; set; }

        public string? Contacts { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }

        [NotMapped]
        public string? Roles { get; set; }
        public DateTime? DateTimeCreated { get; set; }
        public DateTime? DateTimeModified { get; set; }
        public bool? IsDeleted { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? TenantId { get; set; }

        /// <summary>
        /// Controls multi-tenant visibility. Null or Private = owning tenant only.
        /// </summary>
        public Access? Access { get; set; }

        /// <summary>
        /// Indicates the user has been approved as an employee.
        /// Set automatically when email matches the employer domain, or when
        /// two admin approvals have been received via tbl_EmployeeApproval.
        /// </summary>
        public bool IsEmployee { get; set; } = false;
    }
}
