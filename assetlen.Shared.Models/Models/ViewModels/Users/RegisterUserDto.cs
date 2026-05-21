using System.ComponentModel.DataAnnotations;
using assetlen.Shared.Models.Validators;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
    [AtLeastOneRequired("Email", "PhoneNumber")]
    public class RegisterUserDto
    {
        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(4)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? UserName { get; set; }
    }
}
