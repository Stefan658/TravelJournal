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
        IEnumerable<Entry> GetAllByJournal(int journalId);
        Entry GetById(int id);
        void Add(Entry entry);
        void Update(Entry entry);
        void Delete(int id);
    }
}

