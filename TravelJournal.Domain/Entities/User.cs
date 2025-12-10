using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TravelJournal.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }

        public string PasswordHash { get; set; }
        public string Role { get; set; }

        // FK către Subscription
        public int SubscriptionId { get; set; }
        public virtual Subscription Subscription { get; set; }

        // Relații
        public virtual ICollection<Journal> Journals { get; set; }
    }
}


