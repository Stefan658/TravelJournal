using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelJournal.Domain.Entities
{
    [Serializable]
    public class Photo
    {
        public int Id { get; set; }

        public string FilePath { get; set; }

        // FK către Entry
        public int EntryId { get; set; }
        public virtual Entry Entry { get; set; }
    }
}
