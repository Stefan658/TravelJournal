using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelJournal.Web.ViewModels.Journals
{
    public class JournalViewModel
    {
        public int JournalId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPublic { get; set; }

        public DateTime CreatedAt { get; set; }

        public int EntryCount { get; set; }
    }
}

