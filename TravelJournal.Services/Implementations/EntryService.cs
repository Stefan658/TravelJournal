using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Exceptions;
using TravelJournal.Services.Interfaces;

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

            var key = $"entries_journal_{journalId}";
            logger.Info($"[EntryService] GetByJournal journalId={journalId}");

            if (_cache.IsSet(key))
            {
                logger.Info($"[EntryService] Cache HIT {key}");
                return _cache.Get<IEnumerable<Entry>>(key) ?? Enumerable.Empty<Entry>();
            }

            logger.Info($"[EntryService] Cache MISS {key}");

            var entries = _entryAccessor.GetAllByJournal(journalId)
                .Where(e => !e.IsDeleted)
                .ToList();

            _cache.Set(key, entries);
            return entries;
        }

        public IEnumerable<Entry> GetDeletedByJournal(int journalId)
        {
            if (journalId <= 0) throw new ArgumentOutOfRangeException(nameof(journalId));

            logger.Info($"[EntryService] GetDeletedByJournal journalId={journalId}");

            return _entryAccessor.GetAllByJournalIncludingDeleted(journalId)
                .Where(e => e.IsDeleted)
                .ToList();
        }

        public Entry GetById(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            logger.Info($"[EntryService] GetById entryId={id}");
            return _entryAccessor.GetById(id);
        }

        public void Create(Entry entry, int userId)
        {
            logger.Info($"[EntryService] Create userId={userId}, journalId={entry?.JournalId}");

            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (entry.JournalId <= 0) throw new ArgumentOutOfRangeException(nameof(entry.JournalId));
            if (string.IsNullOrWhiteSpace(entry.Title)) throw new ArgumentException("Title is required.", nameof(entry.Title));

            // 1) user exists
            var user = _userAccessor.GetById(userId);
            if (user == null) throw new InvalidOperationException("User not found.");

            var subscription = _subs.GetById(user.SubscriptionId);
            if (subscription == null || !subscription.IsActive)
                throw new SubscriptionLimitException("Your subscription plan is missing or inactive.");


            var existingCount = _entryAccessor.GetAllByJournal(entry.JournalId).Count();

            if (subscription.EntryLimit > 0 && subscription.EntryLimit != int.MaxValue)
            {
                if (existingCount >= subscription.EntryLimit)
                {
                    var msg =
                        $"Entry limit reached for plan '{subscription.Name}'. " +
                        $"Limit={subscription.EntryLimit} entries/journal. Upgrade to add more entries.";
                    logger.Warn($"[EntryService] {msg}");
                    throw new SubscriptionLimitException(msg);
                }
            }

            // 3) set defaults
            entry.UserId = userId;
            if (entry.CreatedAt == default(DateTime)) entry.CreatedAt = DateTime.Now;
            entry.UpdatedAt = DateTime.Now;
            entry.IsDeleted = false;

            // 4) persist + invalidate cache
            _entryAccessor.Add(entry);
            _cache.Remove($"entries_journal_{entry.JournalId}");
        }

        public void Update(Entry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            logger.Info($"[EntryService] Update entryId={entry.EntryId}");

            entry.UpdatedAt = DateTime.Now;
            _entryAccessor.Update(entry);

            _cache.Remove($"entries_journal_{entry.JournalId}");
        }

        public void Delete(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            logger.Info($"[EntryService] SoftDelete entryId={id}");

            var entry = _entryAccessor.GetById(id);
            if (entry == null) return;

            _entryAccessor.SoftDelete(id);
            _cache.Remove($"entries_journal_{entry.JournalId}");
        }

        public void Restore(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            logger.Info($"[EntryService] Restore entryId={id}");

            var entry = _entryAccessor.GetById(id);
            if (entry == null) return;

            _entryAccessor.Restore(id);
            _cache.Remove($"entries_journal_{entry.JournalId}");
        }

        public Entry GetByIdForUser(int entryId, int userId)
        {
            if (entryId <= 0 || userId <= 0) return null;

            var e = _entryAccessor.GetById(entryId);
            if (e == null) return null;

            // user area nu vede deleted
            if (e.IsDeleted) return null;

            return e.UserId == userId ? e : null;
        }
    }
}
