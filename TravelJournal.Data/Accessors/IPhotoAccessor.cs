using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Accessors
{
    public interface IPhotoAccessor
    {
        IEnumerable<Photo> GetByEntry(int entryId);
        void Add(Photo photo);
        void Delete(int id);
    }

}
