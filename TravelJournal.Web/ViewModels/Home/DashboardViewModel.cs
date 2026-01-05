using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace TravelJournal.Web.ViewModels.Home
{
    public class DashboardViewModel
    {
        public int UserId { get; set; }

        public int JournalsCount { get; set; }
        public int EntriesCount { get; set; }

        public List<RecentEntryVm> RecentEntries { get; set; } = new List<RecentEntryVm>();

        public List<TopLocationVm> TopLocations { get; set; } = new List<TopLocationVm>();


        public class RecentEntryVm
        {
            public int EntryId { get; set; }
            public int JournalId { get; set; }
            public string Title { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class TopLocationVm
        {
            public string Location { get; set; }
            public int Count { get; set; }
        }
    }
}
