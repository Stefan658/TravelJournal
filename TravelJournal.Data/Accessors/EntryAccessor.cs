using System;
using System.Collections.Generic;
using System.Linq;
using TravelJournal.Domain.Entities;
using TravelJournal.Data.Context;
using NLog;

namespace TravelJournal.Data.Accessors
{
    public class EntryAccessor : IEntryAccessor
    {
        private readonly TravelJournalDbContext _db;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EntryAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Entry> GetAllByJournal(int journalId)
        {
            logger.Info($"[EntryAccessor] Loading entries for JournalId={journalId}");

            try
            {
                var entries = _db.Entries
                    .Where(e => e.JournalId == journalId && !e.IsDeleted)
                    .ToList();

                logger.Info($"[EntryAccessor] Retrieved {entries.Count} entries for JournalId={journalId}");
                return entries;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryAccessor] Error loading entries for JournalId={journalId}");
                throw;
            }
        }

        public Entry GetById(int id)
        {
            logger.Info($"[EntryAccessor] GetById called for EntryId={id}");

            try
            {
                var entry = _db.Entries.Find(id);

                if (entry == null)
                    logger.Warn($"[EntryAccessor] EntryId={id} not found");

                return entry;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryAccessor] Error in GetById for EntryId={id}");
                throw;
            }
        }

        public void Add(Entry entry)
        {
            logger.Info($"[EntryAccessor] Adding new Entry (JournalId={entry.JournalId}, Title={entry.Title})");

            try
            {
                _db.Entries.Add(entry);
                _db.SaveChanges();
                logger.Info($"[EntryAccessor] Entry added successfully (EntryId={entry.EntryId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[EntryAccessor] Error adding entry");
                throw;
            }
        }

        public void Update(Entry entry)
        {
            logger.Info($"[EntryAccessor] Updating EntryId={entry.EntryId}");

            try
            {
                _db.Entry(entry).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();

                logger.Info($"[EntryAccessor] EntryId={entry.EntryId} updated successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryAccessor] Error updating EntryId={entry.EntryId}");
                throw;
            }
        }

       /* public void Delete(int id)
        {
            logger.Info($"[EntryAccessor] Deleting EntryId={id}");

            try
            {
                var entry = _db.Entries.Find(id);

                if (entry == null)
                {
                    logger.Warn($"[EntryAccessor] Delete failed — EntryId={id} not found");
                    return;
                }

                _db.Entries.Remove(entry);
                _db.SaveChanges();

                logger.Info($"[EntryAccessor] EntryId={id} deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryAccessor] Error deleting EntryId={id}");
                throw;
            }
        }*/

        

        public IEnumerable<Entry> GetAllByJournalIncludingDeleted(int journalId)
        {
            logger.Info($"[EntryAccessor] Loading (incl deleted) entries for JournalId={journalId}");

            try
            {
                return _db.Entries
                    .Where(e => e.JournalId == journalId)
                    .ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryAccessor] Error loading entries (incl deleted) for JournalId={journalId}");
                throw;
            }
        }

        public void SoftDelete(int id)
        {
            logger.Info($"[EntryAccessor] SoftDelete EntryId={id}");

            try
            {
                var entry = _db.Entries.Find(id);
                if (entry == null)
                {
                    logger.Warn($"[EntryAccessor] SoftDelete failed — EntryId={id} not found");
                    return;
                }

                entry.IsDeleted = true;
                _db.SaveChanges();

                logger.Info($"[EntryAccessor] EntryId={id} soft-deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryAccessor] Error soft-deleting EntryId={id}");
                throw;
            }
        }

        public void Restore(int id)
        {
            logger.Info($"[EntryAccessor] Restore EntryId={id}");

            try
            {
                var entry = _db.Entries.Find(id);
                if (entry == null)
                {
                    logger.Warn($"[EntryAccessor] Restore failed — EntryId={id} not found");
                    return;
                }

                entry.IsDeleted = false;
                _db.SaveChanges();

                logger.Info($"[EntryAccessor] EntryId={id} restored successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryAccessor] Error restoring EntryId={id}");
                throw;
            }
        }



    }
}
