using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Web.ViewModels.Subscriptions
{
    public class PlansViewModel
    {
        public int UserId { get; set; }
        public int CurrentSubscriptionId { get; set; }
        public List<Subscription> Plans { get; set; }
    }
}
