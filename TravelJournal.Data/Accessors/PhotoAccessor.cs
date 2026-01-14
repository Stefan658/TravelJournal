using NLog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Data.Context;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Accessors
{
    public class PhotoAccessor : IPhotoAccessor
    {
        private readonly TravelJournalDbContext _db;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PhotoAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Photo> GetByEntry(int entryId)
        {
            logger.Info($"[PhotoAccessor] Loading photos for EntryId={entryId}");
            return _db.Photos.Where(p => p.EntryId == entryId).ToList();
        }

        public void Add(Photo photo)
        {
            _db.Photos.Add(photo);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var photo = _db.Photos.Find(id);
            if (photo == null) return;

            _db.Photos.Remove(photo);
            _db.SaveChanges();
        }
    }

}
