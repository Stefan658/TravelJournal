using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Data.Accessors;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;

namespace TravelJournal.Services.Implementations
{
    public class JournalService : IJournalService
    {
        private readonly IJournalAccessor _journalAccessor;

        public JournalService(IJournalAccessor journalAccessor)
        {
            _journalAccessor = journalAccessor;
        }

        public IEnumerable<Journal> GetByUser(int userId)
        {
            return _journalAccessor.GetAllByUser(userId);
        }

        public Journal GetById(int id)
        {
            return _journalAccessor.GetById(id);
        }

        public void Create(Journal journal)
        {
            _journalAccessor.Add(journal);
        }

        public void Update(Journal journal)
        {
            _journalAccessor.Update(journal);
        }

        public void Delete(int id)
        {
            _journalAccessor.Delete(id);
        }
    }
}
