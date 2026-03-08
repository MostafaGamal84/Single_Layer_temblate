using DTOs;

namespace API.DTOs.ResetPasswordDto
{
    public class ResetPasswordTokenDto : BaseDto
    {
        public string Email { get; set; }
    }
}