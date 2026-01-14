using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Web.ViewModels.Entries
{
    public class EntryViewModel
    {
        public int EntryId { get; set; }
        public int JournalId { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string Location { get; set; }

       // public decimal? Latitude { get; set; }
       // public decimal? Longitude { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IEnumerable<Photo> Photos { get; set; }
        public bool CanUploadPhotos { get; set; }
        public string SubscriptionName { get; set; }
        public bool CanExportPdf { get; set; }



    }
}
