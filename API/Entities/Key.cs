using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;

namespace API.Entities
{
    public class Key : BaseEntity
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}