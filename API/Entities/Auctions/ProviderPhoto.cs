using Entities;

namespace API.Entities.Auctions
{
    public class ProviderPhoto : BaseEntity
    {
        public string CommercialRecordPhotoUrl { get; set; }
        public string CompanyPhotoUrl { get; set; }
        public string IdentityPhotoUrl { get; set; }
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
    }
}