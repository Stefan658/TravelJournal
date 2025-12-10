using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;

namespace TravelJournal.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionAccessor _subs;

        public SubscriptionService(ISubscriptionAccessor subs)
        {
            _subs = subs;
        }

        public IEnumerable<Subscription> GetAll() => _subs.GetAll();

        public Subscription GetById(int id) => _subs.GetById(id);

        public bool CanUploadMedia(int subscriptionId)
        {
            var sub = GetById(subscriptionId);
            return sub.Name == "Premium" || sub.Name == "Explorer";
        }

        public bool CanExportPdf(int subscriptionId)
        {
            var sub = GetById(subscriptionId);
            return sub.Name == "Explorer";
        }

        public bool CanUseMap(int subscriptionId)
        {
            var sub = GetById(subscriptionId);
            return sub.Name == "Premium" || sub.Name == "Explorer";
        }
    }
}

