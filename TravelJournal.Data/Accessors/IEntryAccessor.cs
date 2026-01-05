using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Accessors
{
    public interface IEntryAccessor
    {
        IEnumerable<Entry> GetAllByJournal(int journalId);                 // doar active (!IsDeleted)
        IEnumerable<Entry> GetAllByJournalIncludingDeleted(int journalId); // include IsDeleted
        Entry GetById(int id);
        void Add(Entry entry);
        void Update(Entry entry);

        void SoftDelete(int id);
        void Restore(int id);
    }
}


