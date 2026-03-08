
using System;
using System.ComponentModel.DataAnnotations;
using DTOs;

namespace API.DTOs
{
    public class SocialClientRegisterDto: BaseDto
    {

        [EmailAddress]
        [Required]
        public string Email { get; set; }
   
    }
}