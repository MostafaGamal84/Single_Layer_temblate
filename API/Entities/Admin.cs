using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{[Table("Admin")]
    public class Admin : AppUser
    {
         public virtual ICollection<Auction> Auctions { get; set; }
    }
}