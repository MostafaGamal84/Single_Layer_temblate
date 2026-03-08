using System.Collections.Generic;
using Entities;

namespace API.Entities
{
    public class ProviderType : BaseEntity
    {
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public virtual ICollection<Provider> Provider { get; set; }
    }
}