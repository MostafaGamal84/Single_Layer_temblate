

using System;
using System.ComponentModel.DataAnnotations;


namespace API.DTOs
{
    public class ClientUpdateDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Token { get; set; }
        public string Bank { get; set; }
        public string IbanNumber { get; set; }
        public string MobileNumber { get; set; }
        public string IdentityNumber { get; set; }
        public string DateOfBirth { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Nationality { get; set; }
        public int SubscribeCount { get; set; }
        
        public string IdentityPhotoBase64 { get; set; }

    }
}