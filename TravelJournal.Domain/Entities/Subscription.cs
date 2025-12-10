using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelJournal.Domain.Entities
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }

        public string Name { get; set; }           // Free, Premium, Explorer
        public string Description { get; set; }
        public decimal Price { get; set; }

        public int StorageLimitMB { get; set; }
        public int EntryLimit { get; set; }
        public bool IsActive { get; set; }

        // Relații
        public virtual ICollection<User> Users { get; set; }
    }
}


