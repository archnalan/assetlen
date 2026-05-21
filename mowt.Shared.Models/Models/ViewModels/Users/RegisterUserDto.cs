using System.ComponentModel.DataAnnotations;
using mowt.Shared.Models.Validators;

namespace mowt.Shared.Models.Models.ViewModels.Users
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
