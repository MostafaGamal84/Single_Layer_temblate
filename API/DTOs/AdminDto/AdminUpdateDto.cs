using DTOs;

namespace API.DTOs.AdminDto
{
    public class AdminUpdateDto : BaseDto
    { 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }

    }
}