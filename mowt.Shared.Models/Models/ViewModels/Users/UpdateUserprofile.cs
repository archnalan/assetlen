using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Shared.Models.Models
{
    public class UpdateUserprofile
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [MinLength(4)]
        public string Password { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string? Address { get; set; }
        public string? Aboutme { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }

        [NotMapped]
        public string[] defaultRole { get; set; } = new[] { "LibraryModuleLogin", "CreateCommentsAndFeedback" };

    }
}
