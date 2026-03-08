using System;
using System.Collections;
using System.Collections.Generic;
using Entities;

namespace API.Entities
{
    public class AppUserType : BaseEntity
    {
         public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public virtual ICollection<AppUser> Users { get; set; }
    }
}