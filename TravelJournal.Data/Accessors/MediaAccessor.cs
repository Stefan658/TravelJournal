using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;
using TravelJournal.Data.Context;

namespace TravelJournal.Data.Accessors
{
    public class MediaAccessor : IMediaAccessor
    {
        private readonly TravelJournalDbContext _db;

        public MediaAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Media> GetByEntry(int entryId)
        {
            return _db.Media.Where(m => m.EntryId == entryId).ToList();
        }

        public void Add(Media media)
        {
            _db.Media.Add(media);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var media = _db.Media.Find(id);
            if (media != null)
            {
                _db.Media.Remove(media);
                _db.SaveChanges();
            }
        }
    }
}

