using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;
using TravelJournal.Data.Context;

namespace TravelJournal.Data.Accessors
{
    public class SubscriptionAccessor : ISubscriptionAccessor
    {
        private readonly TravelJournalDbContext _db;

        public SubscriptionAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Subscription> GetAll()
        {
            return _db.Subscriptions.ToList();
        }

        public Subscription GetById(int id)
        {
            return _db.Subscriptions.Find(id);
        }
    }
}

