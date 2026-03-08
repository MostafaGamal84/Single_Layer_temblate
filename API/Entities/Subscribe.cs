using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;

namespace API.Entities
{
    public class Subscribe : BaseEntity
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public int AuctioningCount { get; set; }
        public double Price { get; set; }
        // public virtual ICollection<Client> Clients { get; set; }
    }
}