using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelJournal.Domain.Entities
{
    public class Journal
    {
        public int JournalId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // FK către User
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Relații
        public virtual ICollection<Entry> Entries { get; set; }
    }
}

