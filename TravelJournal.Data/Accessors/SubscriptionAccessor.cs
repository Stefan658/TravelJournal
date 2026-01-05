using System;
using System.Collections.Generic;
using System.Linq;
using TravelJournal.Domain.Entities;
using TravelJournal.Data.Context;
using NLog;

namespace TravelJournal.Data.Accessors
{
    public class SubscriptionAccessor : ISubscriptionAccessor
    {
        private readonly TravelJournalDbContext _db;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SubscriptionAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Subscription> GetAll()
        {
            logger.Info("[SubscriptionAccessor] Retrieving all subscriptions");

            try
            {
                var subs = _db.Subscriptions.ToList();
                logger.Info($"[SubscriptionAccessor] Retrieved {subs.Count} subscriptions");
                return subs;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[SubscriptionAccessor] Error retrieving subscriptions");
                throw;
            }
        }

        public Subscription GetById(int id)
        {
            logger.Info($"[SubscriptionAccessor] GetById called for SubscriptionId={id}");

            try
            {
                var subscription = _db.Subscriptions.Find(id);

                if (subscription == null)
                    logger.Warn($"[SubscriptionAccessor] SubscriptionId={id} not found");

                return subscription;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[SubscriptionAccessor] Error retrieving SubscriptionId={id}");
                throw;
            }
        }
    }
}
