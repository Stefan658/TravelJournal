using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelJournal.Domain.Entities
{
    public class Media
    {
        public int MediaId { get; set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
        public string Url { get; set; }

        public DateTime UploadedAt { get; set; }

        // FK
        public int EntryId { get; set; }
        public virtual Entry Entry { get; set; }
    }
}
