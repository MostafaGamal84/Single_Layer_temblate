using System;
using System.Collections.Generic;

using API.DTOs.GenderDTO;

using DTOs;

namespace API.DTOs
{
    public class ClientReturnDto : BaseDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IbanNumber { get; set; }
        public string MobileNumber { get; set; }
        public string Bank { get; set; }
        public string DateOfBirth { get; set; }
 
        public string IdentityNumber { get; set; }
        public string Nationality { get; set; }
        public string City { get; set; }
        public string IdentityPhotoUrl { get; set; }
        public int SubscribeCount { get; set; }
        public virtual  SubscribeDto Subscribe { get; set; }

    }
}
