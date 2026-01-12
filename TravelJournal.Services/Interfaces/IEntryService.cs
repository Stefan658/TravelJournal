using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Services.Interfaces
{
    public interface IEntryService
    {
        IEnumerable<Entry> GetByJournal(int journalId);
        IEnumerable<Entry> GetDeletedByJournal(int journalId);

        Entry GetById(int id);
        void Create(Entry entry, int userId);
        void Update(Entry entry);
        void Delete(int id);   // soft delete
        void Restore(int id);  // restore
        Entry GetByIdForUser(int entryId, int userId);

    }
}

