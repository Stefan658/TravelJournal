using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;
using TravelJournal.Data.Context;

namespace TravelJournal.Data.Accessors
{
    public class EntryAccessor : IEntryAccessor
    {
        private readonly TravelJournalDbContext _db;

        public EntryAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Entry> GetAllByJournal(int journalId)
        {
            return _db.Entries.Where(e => e.JournalId == journalId).ToList();
        }

        public Entry GetById(int id)
        {
            return _db.Entries.Find(id);
        }

        public void Add(Entry entry)
        {
            _db.Entries.Add(entry);
            _db.SaveChanges();
        }

        public void Update(Entry entry)
        {
            _db.Entry(entry).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var entry = _db.Entries.Find(id);
            if (entry != null)
            {
                _db.Entries.Remove(entry);
                _db.SaveChanges();
            }
        }
    }
}
