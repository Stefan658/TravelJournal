using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelJournal.Web.ViewModels.Subscriptions
{
    public class MySubscriptionViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }

        public string CurrentPlanName { get; set; }
        public decimal CurrentPlanPrice { get; set; }

        public bool CanUploadMedia { get; set; }
        public bool CanExportPdf { get; set; }
        public bool CanUseMap { get; set; }
    }
}
