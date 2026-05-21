
using mowt.Shared.Models.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mowt.Shared.Models.Models
{
    public class CreateUserResponseDto : BaseDto
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string PhoneNumber { get; set; }


        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string? Address { get; set; }
        public string? Aboutme { get; set; }
        public string? Industry { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }


        public string[] defaultRole { get; set; } = new[] { "LibraryModuleLogin", "CreateCommentsAndFeedback" };
        public bool IsEmployee { get; set; }
    }
}
