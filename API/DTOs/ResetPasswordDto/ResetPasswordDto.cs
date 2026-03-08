using System.ComponentModel.DataAnnotations;
using DTOs;

namespace API.DTOs.ResetPasswordDto
{
    public class ResetPasswordDto : BaseDto
    {
        public string Email { get; set; }

        [Required]
        public string NewPassword { get; set; }

        

        public string Token { get; set; }
    }
    
}