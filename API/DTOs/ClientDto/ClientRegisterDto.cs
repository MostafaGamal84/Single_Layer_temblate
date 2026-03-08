
using System;
using System.ComponentModel.DataAnnotations;


namespace API.DTOs
{
    public class ClientRegisterDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Bank { get; set; }
        public string IbanNumber { get; set; }
        public string MobileNumber { get; set; }
        public string IdentityPhotoBase64 { get; set; }

        public string IdentityNumber { get; set; }
        public string DateOfBirth { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string Nationality { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string ConfirmPassword { get; set; }
    }
}