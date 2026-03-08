using System.ComponentModel.DataAnnotations;
using DTOs;

namespace API.DTOs.ClientDto
{
    public class SocialClientReturnDto : BaseDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string Token { get; set; }
    }
}