using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using TravelJournal.Services.Exceptions; // <-- adauga namespace-ul tau pentru SubscriptionLimitException

namespace TravelJournal.Services.Implementations
{
    public class EntryService : IEntryService
    {
        private readonly IEntryAccessor _entryAccessor;
        private readonly IUserAccessor _userAccessor;
        private readonly ISubscriptionService _subs;
        private readonly ICache _cache;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EntryService(
            IEntryAccessor entryAccessor,
            IUserAccessor userAccessor,
            ISubscriptionService subs,
            ICache cache)
        {
            _entryAccessor = entryAccessor ?? throw new ArgumentNullException(nameof(entryAccessor));
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
            _subs = subs ?? throw new ArgumentNullException(nameof(subs));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IEnumerable<Entry> GetByJournal(int journalId)
        {
            if (journalId <= 0) throw new ArgumentOutOfRangeException(nameof(journalId));

            string key = $"entries_journal_{journalId}";
            logger.Info($"[EntryService] Retrieving entries for JournalId={journalId}");

            try
            {
                if (_cache.IsSet(key))
                {
                    logger.Info($"[EntryService] Cache HIT for {key}");
                    return _cache.Get<IEnumerable<Entry>>(key) ?? Enumerable.Empty<Entry>();
                }

                logger.Info($"[EntryService] Cache MISS for {key}");

                // materializez ca sa evit multiple enumerations si sa cache-uiesc stabil
                var entries = _entryAccessor.GetAllByJournal(journalId).ToList();
                _cache.Set(key, entries);

                logger.Info($"[EntryService] Found {entries.Count} entries for JournalId={journalId}");
                return entries;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryService] Error retrieving entries for JournalId={journalId}");
                throw;
            }
        }

        public Entry GetById(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            logger.Info($"[EntryService] GetById called for EntryId={id}");

            try
            {
                var entry = _entryAccessor.GetById(id);

                if (entry == null)
                    logger.Warn($"[EntryService] EntryId={id} not found");

                return entry;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryService] Error in GetById for EntryId={id}");
                throw;
            }
        }

        public void Create(Entry entry, int userId)
        {
            logger.Info($"[EntryService] Creating entry for UserId={userId}, JournalId={entry?.JournalId}");

            // validari rapide
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (entry.JournalId <= 0) throw new ArgumentOutOfRangeException(nameof(entry.JournalId));
            if (string.IsNullOrWhiteSpace(entry.Title)) throw new ArgumentException("Title is required.", nameof(entry.Title));

            try
            {
                // 1) user + subscription
                var user = _userAccessor.GetById(userId);
                if (user == null)
                {
                    logger.Warn($"[EntryService] UserId={userId} not found.");
                    throw new InvalidOperationException("User not found.");
                }

                var subscription = _subs.GetById(user.SubscriptionId);
                if (subscription == null || !subscription.IsActive)
                {
                    logger.Warn($"[EntryService] Subscription missing/inactive for UserId={userId}, SubscriptionId={user.SubscriptionId}");
                    throw new SubscriptionLimitException("Your subscription plan is missing or inactive. Please choose a valid plan.");
                }

                // 2) gating: limita entries/jurnal
                // (folosim accessor direct ca sa fie sursa de adevar, nu cache)
                var existingEntriesCount = _entryAccessor.GetAllByJournal(entry.JournalId).Count();

                // EntryLimit: 0 sau int.MaxValue = nelimitat (conform seed-ului tau)
                if (subscription.EntryLimit > 0 && subscription.EntryLimit != int.MaxValue)
                {
                    if (existingEntriesCount >= subscription.EntryLimit)
                    {
                        var msg =
                            $"Entry limit reached for plan '{subscription.Name}'. " +
                            $"Limit={subscription.EntryLimit} entries/journal. " +
                            $"Upgrade to add more entries.";

                        logger.Warn($"[EntryService] {msg} (UserId={userId}, JournalId={entry.JournalId}, Current={existingEntriesCount})");
                        throw new SubscriptionLimitException(msg);
                    }
                }

                // 3) setari standard entry (daca nu sunt deja setate in controller)
                entry.UserId = userId;
                if (entry.CreatedAt == default(DateTime)) entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;

                // 4) persist + invalidate cache
                _entryAccessor.Add(entry);

                // invalidam strict jurnalul afectat
                _cache.Remove($"entries_journal_{entry.JournalId}");

                logger.Info($"[EntryService] Entry created successfully (EntryId={entry.EntryId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[EntryService] Error creating entry");
                throw;
            }
        }

        public void Update(Entry entry)
        {
            logger.Info($"[EntryService] Updating EntryId={entry?.EntryId}");

            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (entry.EntryId <= 0) throw new ArgumentOutOfRangeException(nameof(entry.EntryId));
            if (entry.JournalId <= 0) throw new ArgumentOutOfRangeException(nameof(entry.JournalId));
            if (string.IsNullOrWhiteSpace(entry.Title)) throw new ArgumentException("Title is required.", nameof(entry.Title));

            try
            {
                entry.UpdatedAt = DateTime.Now;

                _entryAccessor.Update(entry);

                // invalidam cache pentru jurnalul entry-ului
                _cache.Remove($"entries_journal_{entry.JournalId}");

                logger.Info($"[EntryService] EntryId={entry.EntryId} updated successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryService] Error updating EntryId={entry.EntryId}");
                throw;
            }
        }

        public IEnumerable<Entry> GetDeletedByJournal(int journalId)
        {
            if (journalId <= 0) throw new ArgumentOutOfRangeException(nameof(journalId));

            // nu cache-uim asta (e admin/bonus)
            return _entryAccessor.GetAllByJournalIncludingDeleted(journalId)
                .Where(e => e.IsDeleted)
                .ToList();
        }


        public void Delete(int id)
        {
            logger.Info($"[EntryService] Soft deleting EntryId={id}");
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                var entry = _entryAccessor.GetById(id);

                _entryAccessor.SoftDelete(id);

                if (entry != null && entry.JournalId > 0)
                    _cache.Remove($"entries_journal_{entry.JournalId}");
                else
                    _cache.RemoveByPattern("entries_journal_");

                logger.Info($"[EntryService] EntryId={id} soft-deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryService] Error soft-deleting EntryId={id}");
                throw;
            }
        }

        public void Restore(int id)
        {
            logger.Info($"[EntryService] Restoring EntryId={id}");
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                var entry = _entryAccessor.GetById(id);

                _entryAccessor.Restore(id);

                if (entry != null && entry.JournalId > 0)
                    _cache.Remove($"entries_journal_{entry.JournalId}");
                else
                    _cache.RemoveByPattern("entries_journal_");

                logger.Info($"[EntryService] EntryId={id} restored successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryService] Error restoring EntryId={id}");
                throw;
            }
        }


    }
}
