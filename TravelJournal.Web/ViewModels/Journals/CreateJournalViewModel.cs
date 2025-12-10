using System.ComponentModel.DataAnnotations;

namespace TravelJournal.Web.ViewModels.Journals
{
    public class CreateJournalViewModel
    {
        public int JournalId { get; set; }   
        public int UserId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPublic { get; set; }
    }
}

