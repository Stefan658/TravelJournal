using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TravelJournal.Web.ViewModels.Entries
{
    public class CreateEntryViewModel
    {
        public int EntryId { get; set; }
        public int JournalId { get; set; }
        public int UserId { get; set; }

        [Required]
        [MinLength(3)]
        public string Title { get; set; }

        [Required]
        [MinLength(5)]
        public string Content { get; set; }

        public string Location { get; set; }

        //public decimal? Latitude { get; set; }
       // public decimal? Longitude { get; set; }

    }
}
