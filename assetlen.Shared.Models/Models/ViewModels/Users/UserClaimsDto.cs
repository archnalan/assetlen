

using assetlen.Shared.Models.Models.ViewModels;

namespace assetlen.Shared.Models.Models
{
    public class UserClaimsDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? UseName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Industry { get; set; }
        public string? Address { get; set; }
        public string? Aboutme { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        //public string? UserId { get; set; }
        public string? Id { get; set; }
        public string? TenantId { get; set; }
        public string? Roles { get; set; }
        public UserRolesDto? RolesDto { get; set; }
        public string? Package { get; set; }

        public UserClaimsDto()
        {

        }

    }

}
