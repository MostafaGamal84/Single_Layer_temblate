using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.Auctions;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    [Table("Provider")]
    public class Provider : AppUser
    {
        public string IdentityNumber { get; set; }
        public string IbanNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string AboutCompany { get; set; }
        public string CompanyServices { get; set; }
        public string CompanyWebSite { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPending { get; set; }
        public string City { get; set; }
        public string Nationality { get; set; }
        public int ProviderTypeId { get; set; }
        public virtual ProviderPhoto ProviderPhotos { get; set; }
        public virtual ProviderType ProviderType { get; set; }
        public virtual ICollection<Auction> Auctions { get; set; }
      
    }
}