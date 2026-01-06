using System;
using System.Collections.Generic;
using System.Linq;
using TravelJournal.Data.Context;
using TravelJournal.Domain.Entities;
using NLog;
using System.Data.Entity;

namespace TravelJournal.Data.Accessors
{
    public class JournalAccessor : IJournalAccessor
    {
        private readonly TravelJournalDbContext _db;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public JournalAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Journal> GetAllByUser(int userId)
        {
            logger.Info($"[JournalAccessor] Loading journals for UserId={userId}");

            try
            {
                var journals = _db.Journals.Where(j => j.UserId == userId).ToList();
                logger.Info($"[JournalAccessor] Retrieved {journals.Count} journals for UserId={userId}");
                return journals;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalAccessor] Error loading journals for UserId={userId}");
                throw;
            }
        }

        public IEnumerable<Journal> GetAll(bool includeEntries = false)
        {
            IQueryable<Journal> q = _db.Journals;

            if (includeEntries)
                q = q.Include(j => j.Entries);

            return q.ToList();
        }


        public Journal GetById(int id)
        {
            logger.Info($"[JournalAccessor] GetById called for JournalId={id}");

            try
            {
                var journal = _db.Journals.Find(id);

                if (journal == null)
                    logger.Warn($"[JournalAccessor] JournalId={id} not found");

                return journal;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalAccessor] Error in GetById for JournalId={id}");
                throw;
            }
        }

        public void Add(Journal journal)
        {
            logger.Info($"[JournalAccessor] Adding journal for UserId={journal.UserId}");

            try
            {
                _db.Journals.Add(journal);
                _db.SaveChanges();
                logger.Info($"[JournalAccessor] Journal added successfully (Id={journal.JournalId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[JournalAccessor] Error adding journal");
                throw;
            }
        }

        public void Update(Journal journal)
        {
            logger.Info($"[JournalAccessor] Updating JournalId={journal.JournalId}");

            try
            {
                _db.Entry(journal).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                logger.Info($"[JournalAccessor] JournalId={journal.JournalId} updated successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalAccessor] Error updating JournalId={journal.JournalId}");
                throw;
            }
        }

        public void Delete(int id)
        {
            logger.Info($"[JournalAccessor] Deleting JournalId={id}");

            try
            {
                var journal = _db.Journals.Find(id);

                if (!(_db.Journals.Find(id) is Journal found))
                {
                    logger.Warn($"[JournalAccessor] Delete failed — JournalId={id} not found");
                    return;
                }

                _db.Journals.Remove(found);
                _db.SaveChanges();
                logger.Info($"[JournalAccessor] JournalId={id} deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalAccessor] Error deleting JournalId={id}");
                throw;
            }
        }
    }
}
