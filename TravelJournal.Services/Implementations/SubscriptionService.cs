using System;
using System.Collections.Generic;
using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using NLog;

namespace TravelJournal.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionAccessor _subs;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SubscriptionService(ISubscriptionAccessor subs)
        {
            _subs = subs;
        }

        public IEnumerable<Subscription> GetAll()
        {
            logger.Info("[SubscriptionService] Retrieving all subscriptions");

            try
            {
                return _subs.GetAll();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[SubscriptionService] Error retrieving subscriptions");
                throw;
            }
        }

        public Subscription GetById(int id)
        {
            logger.Info($"[SubscriptionService] GetById called for SubscriptionId={id}");

            try
            {
                var sub = _subs.GetById(id);

                if (sub == null)
                    logger.Warn($"[SubscriptionService] SubscriptionId={id} not found");

                return sub;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[SubscriptionService] Error in GetById for SubscriptionId={id}");
                throw;
            }
        }

        public bool CanUploadMedia(int subscriptionId)
        {
            logger.Info($"[SubscriptionService] Checking upload rights for SubscriptionId={subscriptionId}");

            try
            {
                var sub = GetById(subscriptionId);
                return sub.Name == "Premium" || sub.Name == "Explorer";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[SubscriptionService] Error checking upload rights");
                throw;
            }
        }

        public bool CanExportPdf(int subscriptionId)
        {
            logger.Info($"[SubscriptionService] Checking PDF export for SubscriptionId={subscriptionId}");

            try
            {
                var sub = GetById(subscriptionId);
                return sub.Name == "Explorer" || sub.Name == "Premium";

            }
            catch (Exception ex)
            {
                logger.Error(ex, "[SubscriptionService] Error checking PDF export rights");
                throw;
            }
        }

        public bool CanUseMap(int subscriptionId)
        {
            logger.Info($"[SubscriptionService] Checking map access for SubscriptionId={subscriptionId}");

            try
            {
                var sub = GetById(subscriptionId);
                return sub.Name == "Premium" || sub.Name == "Explorer";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[SubscriptionService] Error checking map rights");
                throw;
            }
        }
    }
}
