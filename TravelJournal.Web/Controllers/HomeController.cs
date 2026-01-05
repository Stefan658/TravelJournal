using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.ViewModels.Home;

namespace TravelJournal.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IJournalService _journalService;
        private readonly IEntryService _entryService;

        // temporar pana la auth
        private const int DefaultUserId = 1;

        public HomeController(IJournalService journalService, IEntryService entryService)
        {
            _journalService = journalService;
            _entryService = entryService;
        }



        // GET: /Home/Dashboard?userId=1
        public ActionResult Dashboard(int? userId)
        {
            int uid = userId ?? DefaultUserId;

            // 🔹 1. Jurnale (cu cache din JournalService)
            var journals = _journalService.GetByUser(uid).ToList();
            int journalsCount = journals.Count;

            // 🔹 2. Entries (agregate pe jurnale)
            var allEntries = journals
                .SelectMany(j => _entryService.GetByJournal(j.JournalId))
                .ToList();

            int entriesCount = allEntries.Count;

            // 🔹 3. Recent entries
            var recentEntries = allEntries
                .OrderByDescending(e => e.CreatedAt)
                .Take(6)
                .Select(e => new DashboardViewModel.RecentEntryVm
                {
                    EntryId = e.EntryId,
                    JournalId = e.JournalId,
                    Title = string.IsNullOrWhiteSpace(e.Title) ? "(no title)" : e.Title,
                    CreatedAt = e.CreatedAt
                })
                .ToList();

            // 🔹 4. Top locații (Entry.Location)
            var topLocations = allEntries
                .Where(e => !string.IsNullOrWhiteSpace(e.Location))
                .GroupBy(e => e.Location.Trim())
                .Select(g => new DashboardViewModel.TopLocationVm
                {
                    Location = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var vm = new DashboardViewModel
            {
                UserId = uid,
                JournalsCount = journalsCount,
                EntriesCount = entriesCount,
                RecentEntries = recentEntries,
                TopLocations = topLocations
            };

            return View(vm);
        }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}