using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelJournal.Web.ViewModels.Photos
{
    public class PhotoViewModel
    {
        public int PhotoId { get; set; }
        public int EntryId { get; set; }

        public string Url { get; set; }
        public string Caption { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
