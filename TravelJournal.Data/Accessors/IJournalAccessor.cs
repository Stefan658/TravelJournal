using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Accessors
{
    public interface IJournalAccessor
    {
        IEnumerable<Journal> GetAllByUser(int userId);
        Journal GetById(int id);
        void Add(Journal journal);
        void Update(Journal journal);
        void Delete(int id);
    }
}
