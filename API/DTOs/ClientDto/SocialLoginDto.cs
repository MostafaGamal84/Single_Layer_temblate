using System.ComponentModel.DataAnnotations;

namespace API.DTOs.ClientDto
{
    public class SocialLoginDto
    {

        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}