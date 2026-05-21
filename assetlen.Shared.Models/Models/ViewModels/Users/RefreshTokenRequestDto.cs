using System.ComponentModel.DataAnnotations;

namespace assetlen.Shared.Models.Models.ViewModels.Users
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string Token { get; set; }
        
        [Required]
        public string RefreshToken { get; set; }
    }
}