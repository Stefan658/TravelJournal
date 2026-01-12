using System;
using System.Collections.Generic;
using System.Linq;
using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using NLog;


namespace TravelJournal.Services.Implementations
{
    public class JournalService : IJournalService
    {
        private readonly IJournalAccessor _journalAccessor;
        private readonly ICache _cache;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public JournalService(IJournalAccessor journalAccessor, ICache cache)
        {
            _journalAccessor = journalAccessor;
            _cache = cache;

        }

        public IEnumerable<Journal> GetByUser(int userId)
        {
            string key = $"journals_user_{userId}";
            logger.Info($"[JournalService] Retrieving journals for UserId={userId}");

            try
            {
                if (_cache.IsSet(key))
                {
                    logger.Info($"[JournalService] Cache HIT for {key}");
                    return _cache.Get<IEnumerable<Journal>>(key);
                }

                logger.Info($"[JournalService] Cache MISS for {key}");
                var journals = _journalAccessor.GetAllByUser(userId).ToList();
                _cache.Set(key, journals);

                return journals;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalService] Error retrieving journals for UserId={userId}");
                throw;
            }
        }


        public Journal GetById(int id)
        {
            string key = $"journal_{id}";
            logger.Info($"[JournalService] GetById called for JournalId={id}");

            try
            {
                if (_cache.IsSet(key))
                {
                    logger.Info($"[JournalService] Cache HIT for {key}");
                    return _cache.Get<Journal>(key);
                }

                logger.Info($"[JournalService] Cache MISS for {key}");
                var journal = _journalAccessor.GetById(id);

                if (journal != null)
                    _cache.Set(key, journal);
                else
                    logger.Warn($"[JournalService] JournalId={id} not found");

                return journal;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalService] Error in GetById for JournalId={id}");
                throw;
            }
        }


        public void Create(Journal journal)
        {
            logger.Info($"[JournalService] Creating journal for UserId={journal.UserId}");

            try
            {
                _journalAccessor.Add(journal);
                _cache.RemoveByPattern("journals_user_");

                logger.Info($"[JournalService] Journal created successfully (Id={journal.JournalId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[JournalService] Error creating journal");
                throw;
            }
        }

        public void Update(Journal journal)
        {
            logger.Info($"[JournalService] Updating JournalId={journal.JournalId}");

            try
            {
                _journalAccessor.Update(journal);
                _cache.Remove($"journal_{journal.JournalId}");
                _cache.RemoveByPattern("journals_user_");

                logger.Info($"[JournalService] JournalId={journal.JournalId} updated successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalService] Error updating JournalId={journal.JournalId}");
                throw;
            }
        }

        public void Delete(int id)
        {
            logger.Info($"[JournalService] Deleting JournalId={id}");

            try
            {
                _journalAccessor.Delete(id);
                _cache.Remove($"journal_{id}");
                _cache.RemoveByPattern("journals_user_");

                logger.Info($"[JournalService] JournalId={id} deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalService] Error deleting JournalId={id}");
                throw;
            }
        }


        public IEnumerable<Journal> GetAll()
        {
            // IMPORTANT: ideal cu Entries incluse ca sa ai EntryCount corect in Admin
            return _journalAccessor.GetAll(includeEntries: true);
        }

        public Journal GetByIdForUser(int journalId, int userId)
        {
            // fără cache la început (siguranță > optimizare)
            var j = _journalAccessor.GetById(journalId);
            if (j == null) return null;
            return j.UserId == userId ? j : null;
        }

    }
}
