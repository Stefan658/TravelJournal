using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Data.Context;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Accessors
{
    public class JournalAccessor : IJournalAccessor
    {
        private readonly TravelJournalDbContext _db;

        public JournalAccessor(TravelJournalDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Journal> GetAllByUser(int userId)
        {
            return _db.Journals.Where(j => j.UserId == userId).ToList();
        }

        public Journal GetById(int id)
        {
            return _db.Journals.Find(id);
        }

        public void Add(Journal journal)
        {
            _db.Journals.Add(journal);
            _db.SaveChanges();
        }

        public void Update(Journal journal)
        {
            _db.Entry(journal).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var journal = _db.Journals.Find(id);
            if (journal != null)
            {
                _db.Journals.Remove(journal);
                _db.SaveChanges();
            }
        }
    }
}

