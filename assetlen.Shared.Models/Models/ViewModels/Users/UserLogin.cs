using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models
{
    public class UserLogin
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [MinLength(4, ErrorMessage = "Password must be at least 4 characters long.")]
        public string Password { get; set; }

    }
}
