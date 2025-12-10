using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelJournal.Domain.Entities;

namespace TravelJournal.Services.Interfaces
{
    public interface IMediaService
    {
        IEnumerable<Media> GetByEntry(int entryId);
        void Upload(Media media, int userId);
        void Delete(int id);
    }
}
