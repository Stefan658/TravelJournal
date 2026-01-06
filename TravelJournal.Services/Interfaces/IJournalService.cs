using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Services.Interfaces
{
    public interface IJournalService
    {
        IEnumerable<Journal> GetByUser(int userId);
        IEnumerable<Journal> GetAll();

        Journal GetById(int id);
        void Create(Journal journal);
        void Update(Journal journal);
        void Delete(int id);
    }
}

