using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Accessors
{
    public interface ISubscriptionAccessor
    {
        IEnumerable<Subscription> GetAll();
        Subscription GetById(int id);
    }
}

