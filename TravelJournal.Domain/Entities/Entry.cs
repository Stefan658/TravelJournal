using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelJournal.Domain.Entities
{
    public class Entry
    {
        public int EntryId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public string Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // FK către Journal
        public int JournalId { get; set; }
        public virtual Journal Journal { get; set; }

        public int UserId { get; set; }


        // Relații
        public virtual ICollection<Media> Media { get; set; }
    }
}


