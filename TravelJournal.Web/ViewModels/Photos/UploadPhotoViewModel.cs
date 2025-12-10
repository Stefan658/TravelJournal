using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TravelJournal.Web.ViewModels.Photos
{
    public class UploadPhotoViewModel
    {
        public int EntryId { get; set; }

        [Required]
        public HttpPostedFileBase File { get; set; }

        public string Caption { get; set; }
    }
}
