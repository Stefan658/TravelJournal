using System;
using System.Data.Entity.Migrations;
using System.Linq;

using TravelJournal.Domain.Entities;

namespace TravelJournal.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<TravelJournal.Data.Context.TravelJournalDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TravelJournal.Data.Context.TravelJournalDbContext context)
        {
            // ──────────────────────────────────────────────
            // 0. Subscription planuri de bază (obligatoriu!)
            // ──────────────────────────────────────────────
            var freePlan = new Subscription
            {
                Name = "Free",
                Description = "Plan gratuit cu limitări.",
                Price = 0,
                StorageLimitMB = 200,
                EntryLimit = 50,
                IsActive = true
            };

            var explorerPlan = new Subscription
            {
                Name = "Explorer",
                Description = "Pentru utilizatori activi, cu export PDF.",
                Price = 9.99m,
                StorageLimitMB = 1000,
                EntryLimit = 50,
                IsActive = true
            };

            var premiumPlan = new Subscription
            {
                Name = "Premium",
                Description = "Acces complet, fără limitări.",
                Price = 19.99m,
                StorageLimitMB = 5000,
                EntryLimit = int.MaxValue,
                IsActive = true
            };


            context.Subscriptions.AddOrUpdate(s => s.Name,freePlan ,explorerPlan, premiumPlan);
            context.SaveChanges();

            // Reîncarcă pentru a obține ID-ul
            freePlan = context.Subscriptions.First(s => s.Name == "Free");


            // ──────────────────────────────────────────────
            // 1. Utilizatori
            // ──────────────────────────────────────────────

            var user1 = new User
            {
                Username = "andrei",
                DisplayName = "Andrei Popescu",
                Email = "andrei@test.com",
                PasswordHash = "test123",
                Role = "User",
                SubscriptionId = freePlan.SubscriptionId
            };

            var user2 = new User
            {
                Username = "maria",
                DisplayName = "Maria Ionescu",
                Email = "maria@test.com",
                PasswordHash = "test123",
                Role = "User",
                SubscriptionId = freePlan.SubscriptionId
            };

            context.Users.AddOrUpdate(u => u.Email, user1, user2);
            context.SaveChanges();

            // Reîncarcă utilizatorii pentru UserId
            user1 = context.Users.First(u => u.Email == "andrei@test.com");
            user2 = context.Users.First(u => u.Email == "maria@test.com");


            // ──────────────────────────────────────────────
            // 2. Jurnale
            // ──────────────────────────────────────────────

            var journal1 = new Journal
            {
                Title = "City Break în Roma",
                Description = "Un weekend superb în Italia.",
                CreatedAt = DateTime.Now.AddDays(-5),
                UpdatedAt = DateTime.Now.AddDays(-5),
                IsPublic = false,
                UserId = user1.UserId
            };

            var journal2 = new Journal
            {
                Title = "Vacanță în Grecia",
                Description = "Plaje superbe și mâncare excelentă!",
                CreatedAt = DateTime.Now.AddDays(-4),
                UpdatedAt = DateTime.Now.AddDays(-4),
                IsPublic = true,
                UserId = user2.UserId
            };

            context.Journals.AddOrUpdate(j => j.Title, journal1, journal2);
            context.SaveChanges();

            // Reîncarcă jurnalele
            journal1 = context.Journals.First(j => j.Title == "City Break în Roma");
            journal2 = context.Journals.First(j => j.Title == "Vacanță în Grecia");


            // ──────────────────────────────────────────────
            // 3. Entries
            // ──────────────────────────────────────────────

            var entry1 = new Entry
            {
                Title = "Ziua 1",
                Content = "Vizită la Colosseum și Fontana di Trevi.",
                Location = "Roma",
                Latitude = 41.8902m,
                Longitude = 12.4922m,
                CreatedAt = DateTime.Now.AddDays(-5),
                UpdatedAt = DateTime.Now.AddDays(-5),
                JournalId = journal1.JournalId,
                UserId = user1.UserId
            };

            var entry2 = new Entry
            {
                Title = "Ziua 2",
                Content = "Muzeele Vaticanului + pizza autentică!",
                Location = "Vatican",
                Latitude = 41.9029m,
                Longitude = 12.4534m,
                CreatedAt = DateTime.Now.AddDays(-4),
                UpdatedAt = DateTime.Now.AddDays(-4),
                JournalId = journal1.JournalId,
                UserId = user1.UserId
            };

            var entry3 = new Entry
            {
                Title = "Plajă și relaxare",
                Content = "O zi perfectă la mare.",
                Location = "Santorini",
                Latitude = 36.3932m,
                Longitude = 25.4615m,
                CreatedAt = DateTime.Now.AddDays(-3),
                UpdatedAt = DateTime.Now.AddDays(-3),
                JournalId = journal2.JournalId,
                UserId = user2.UserId
            };

            context.Entries.AddOrUpdate(
                e => new { e.JournalId, e.Title },
                entry1, entry2, entry3
            );

            context.SaveChanges();


            // ──────────────────────────────────────────────
            // 4. Photos — putem seta FilePath = null
            // ──────────────────────────────────────────────

            var photo1 = new Photo
            {
                EntryId = entry1.EntryId,
                FilePath = null   // încă nu avem path real
            };

            var photo2 = new Photo
            {
                EntryId = entry2.EntryId,
                FilePath = null
            };

            var photo3 = new Photo
            {
                EntryId = entry3.EntryId,
                FilePath = null
            };

            context.Photos.AddOrUpdate(p => new { p.EntryId }, photo1, photo2, photo3);
            context.SaveChanges();


            // ──────────────────────────────────────────────
            // 5. Media — putem seta numele / url ca null, fileSize=0
            // ──────────────────────────────────────────────

            var media1 = new Media
            {
                EntryId = entry1.EntryId,
                FileName = null,
                FileType = null,
                FileSize = 0,
                Url = null,
                UploadedAt = DateTime.Now
            };

            var media2 = new Media
            {
                EntryId = entry2.EntryId,
                FileName = null,
                FileType = null,
                FileSize = 0,
                Url = null,
                UploadedAt = DateTime.Now
            };

            var media3 = new Media
            {
                EntryId = entry3.EntryId,
                FileName = null,
                FileType = null,
                FileSize = 0,
                Url = null,
                UploadedAt = DateTime.Now
            };

            context.Media.AddOrUpdate(m => new { m.EntryId }, media1, media2, media3);
            context.SaveChanges();



        }
    }
}
