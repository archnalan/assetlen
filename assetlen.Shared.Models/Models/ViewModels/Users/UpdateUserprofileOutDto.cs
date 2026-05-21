
using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models
{
    public class UpdateUserprofileOutDto
    {

        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Relationship { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Family { get; set; }
        public string? Aboutme { get; set; }
        public string? Industry { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? CoverPicUrl { get; set; }
        public string? UserName { get; set; }

    }
}
