using System.Web.Mvc;
using System.Web.Routing;

namespace TravelJournal.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            /*
             * IMPORTANT:
             * Rutele specifice trebuie puse ÎNAINTEA rutei default.
             * Rutele trebuie să aibă NUME UNICE.
             */

            // ──────────────────────────────────────────────
            // 1. ROUTE: List journals for a user
            // GET: /users/5/journals
            // Maps to JournalController.Index(int userId)
            // ──────────────────────────────────────────────
            routes.MapRoute(
                name: "UserJournals",
                url: "users/{userId}/journals",
                defaults: new { controller = "Journal", action = "Index" }
            );

            // ──────────────────────────────────────────────
            // 2. ROUTE: Journal details
            // GET: /journals/5
            // Maps to JournalController.Details(int id)
            // ──────────────────────────────────────────────
            routes.MapRoute(
                name: "JournalDetails",
                url: "journals/{id}",
                defaults: new { controller = "Journal", action = "Details" }
            );

            // ──────────────────────────────────────────────
            // 3. ROUTE: Entries of a journal
            // GET: /journals/5/entries
            // Maps to EntryController.Index(int journalId)
            // ──────────────────────────────────────────────
            routes.MapRoute(
                name: "JournalEntries",
                url: "journals/{journalId}/entries",
                defaults: new { controller = "Entry", action = "Index" }
            );

            // ──────────────────────────────────────────────
            // 4. ROUTE: Entry details
            // GET: /entries/10
            // Maps to EntryController.Details(int id)
            // ──────────────────────────────────────────────
            routes.MapRoute(
                name: "EntryDetails",
                url: "entries/{id}",
                defaults: new { controller = "Entry", action = "Details" }
            );

            // ──────────────────────────────────────────────
            // 5. ROUTE: Create new entry
            // GET: /journals/5/entries/create
            // Maps to EntryController.Create(int journalId)
            // ──────────────────────────────────────────────
            routes.MapRoute(
                name: "CreateEntry",
                url: "journals/{journalId}/entries/create",
                defaults: new { controller = "Entry", action = "Create" }
            );

            // ------------------------------------------------------------------------------
            // DEFAULT ROUTE — trebuie să fie ultima!!!
            // ------------------------------------------------------------------------------
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
