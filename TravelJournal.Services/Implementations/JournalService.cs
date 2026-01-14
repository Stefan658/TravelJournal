using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;

namespace TravelJournal.Services.Implementations
{
    public class JournalService : IJournalService
    {
        private readonly IJournalAccessor _journalAccessor;
        private readonly ICache _cache;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public JournalService(IJournalAccessor journalAccessor, ICache cache)
        {
            _journalAccessor = journalAccessor ?? throw new ArgumentNullException(nameof(journalAccessor));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IEnumerable<Journal> GetByUser(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var key = $"journals_user_{userId}";
            logger.Info($"[JournalService] GetByUser userId={userId}");

            if (_cache.IsSet(key))
            {
                logger.Info($"[JournalService] Cache HIT {key}");
                return _cache.Get<IEnumerable<Journal>>(key) ?? Enumerable.Empty<Journal>();
            }

            logger.Info($"[JournalService] Cache MISS {key}");
            var journals = _journalAccessor.GetAllByUser(userId).ToList();

            _cache.Set(key, journals);
            return journals;
        }

        public IEnumerable<Journal> GetAll()
        {
            // Admin use-case (nu user area)
            logger.Info("[JournalService] GetAll");
            return _journalAccessor.GetAll(includeEntries: true);
        }

        public Journal GetById(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var key = $"journal_{id}";
            logger.Info($"[JournalService] GetById id={id}");

            if (_cache.IsSet(key))
            {
                logger.Info($"[JournalService] Cache HIT {key}");
                return _cache.Get<Journal>(key);
            }

            logger.Info($"[JournalService] Cache MISS {key}");
            var journal = _journalAccessor.GetById(id);

            _cache.Set(key, journal);
            return journal;
        }

        public void Create(Journal journal)
        {
            if (journal == null) throw new ArgumentNullException(nameof(journal));

            logger.Info($"[JournalService] Create userId={journal.UserId}");

            _journalAccessor.Add(journal);

            // invalidare: lista userului
            _cache.Remove($"journals_user_{journal.UserId}");
        }

        public void Update(Journal journal)
        {
            if (journal == null) throw new ArgumentNullException(nameof(journal));

            logger.Info($"[JournalService] Update journalId={journal.JournalId} userId={journal.UserId}");

            _journalAccessor.Update(journal);

            _cache.Remove($"journal_{journal.JournalId}");
            _cache.Remove($"journals_user_{journal.UserId}");
        }

        public void Delete(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            logger.Info($"[JournalService] Delete journalId={id}");

            // pentru invalidare corecta, aflam userId-ul jurnalului
            var existing = _journalAccessor.GetById(id);

            _journalAccessor.Delete(id);

            _cache.Remove($"journal_{id}");
            if (existing != null)
            {
                _cache.Remove($"journals_user_{existing.UserId}");
            }
            else
            {
                // fallback safe
                _cache.RemoveByPattern("journals_user_");
            }
        }

        // Ownership enforcement (critica)
        public Journal GetByIdForUser(int journalId, int userId)
        {
            if (journalId <= 0 || userId <= 0) return null;

            // fara cache aici (siguranta > perf)
            var j = _journalAccessor.GetById(journalId);
            if (j == null) return null;

            return j.UserId == userId ? j : null;
        }
    }
}
