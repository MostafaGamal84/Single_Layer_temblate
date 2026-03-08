using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDeleted { get; set; }
        public string Bank { get; set; }
        public string MobileNumber { get; set; }
        public DateTime RegisterTime { get; set; }
        public int? AppUserTypeId { get; set; }
        public virtual AppUserType AppUserType { get; set; }
        public virtual ICollection<AppUserRole> UserRoles { get; set; }
    }
}