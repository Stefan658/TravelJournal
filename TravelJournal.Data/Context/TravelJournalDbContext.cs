using System.Data.Entity;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Context
{
    public class TravelJournalDbContext : DbContext
    {
        public TravelJournalDbContext()
            : base("TravelJournalConnection")   
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Photo> Photos{ get; set; }
    }
}
