using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Services.Interfaces
{
    public interface IPhotoService
    {
        IEnumerable<Photo> GetByEntry(int entryId);
        void Upload(Photo photo, int userId);
        void Delete(int photoId, int userId);
    }

}
