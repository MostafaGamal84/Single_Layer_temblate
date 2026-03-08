using DTOs;

namespace API.DTOs.AdminDto
{
    public class AdminReturnDto : BaseDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}