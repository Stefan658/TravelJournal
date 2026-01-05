using System;
using System.Collections.Generic;
using System.Linq;
using TravelJournal.Domain.Entities;
using TravelJournal.Data.Context;
using NLog;

namespace TravelJournal.Data.Accessors
{
    public class MediaAccessor : IMediaAccessor
    {
        private readonly TravelJournalDbContext _db;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public MediaAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Media> GetByEntry(int entryId)
        {
            logger.Info($"[MediaAccessor] Loading media for EntryId={entryId}");

            try
            {
                var media = _db.Media.Where(m => m.EntryId == entryId).ToList();
                logger.Info($"[MediaAccessor] Retrieved {media.Count} media files for EntryId={entryId}");
                return media;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[MediaAccessor] Error loading media for EntryId={entryId}");
                throw;
            }
        }

        public void Add(Media media)
        {
            logger.Info($"[MediaAccessor] Adding media file for EntryId={media.EntryId}");

            try
            {
                _db.Media.Add(media);
                _db.SaveChanges();
                logger.Info($"[MediaAccessor] Media added successfully (MediaId={media.MediaId})");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[MediaAccessor] Error adding media");
                throw;
            }
        }

        public void Delete(int id)
        {
            logger.Info($"[MediaAccessor] Deleting MediaId={id}");

            try
            {
                var media = _db.Media.Find(id);

                if (media == null)
                {
                    logger.Warn($"[MediaAccessor] Delete failed — MediaId={id} not found");
                    return;
                }

                _db.Media.Remove(media);
                _db.SaveChanges();
                logger.Info($"[MediaAccessor] MediaId={id} deleted successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[MediaAccessor] Error deleting MediaId={id}");
                throw;
            }
        }
    }
}
