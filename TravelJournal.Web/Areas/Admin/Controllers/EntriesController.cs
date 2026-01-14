using System;
using System.Linq;
using System.Web.Mvc;

using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.Infrastructure;

namespace TravelJournal.Web.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class EntriesController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly IJournalService _journalService;

        public EntriesController(IEntryService entryService, IJournalService journalService)
        {
            _entryService = entryService ?? throw new ArgumentNullException(nameof(entryService));
            _journalService = journalService ?? throw new ArgumentNullException(nameof(journalService));
        }

        // GET: Admin/Entries?journalId=1
        // IMPORTANT: journalId optional ca sa nu crape la /Admin/Entries (fara querystring)
        public ActionResult Index(int? journalId)
        {
            if (!journalId.HasValue || journalId.Value <= 0)
                return RedirectToAction("Index", "Journals"); // /Admin/Journals

            var journal = _journalService.GetById(journalId.Value);
            if (journal == null)
                return HttpNotFound("Journal not found.");

            var entries = _entryService.GetByJournal(journalId.Value)
                ?.Where(e => !e.IsDeleted)
                .ToList() ?? Enumerable.Empty<Entry>();

            ViewBag.JournalId = journalId.Value;
            ViewBag.JournalTitle = journal.Title;

            return View(entries);
        }

        // GET: Admin/Entries/Details/5
        public ActionResult Details(int id)
        {
            var entry = _entryService.GetById(id);
            if (entry == null) return HttpNotFound("Entry not found.");

            ViewBag.JournalId = entry.JournalId;
            return View(entry);
        }

        // GET: Admin/Entries/Create?journalId=1
        public ActionResult Create(int? journalId)
        {
            if (!journalId.HasValue || journalId.Value <= 0)
                return RedirectToAction("Index", "Journals");

            var journal = _journalService.GetById(journalId.Value);
            if (journal == null) return HttpNotFound("Journal not found.");

            ViewBag.JournalId = journalId.Value;
            ViewBag.JournalTitle = journal.Title;

            return View(new Entry
            {
                JournalId = journalId.Value
            });
        }

        // POST: Admin/Entries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Entry entry)
        {
            if (entry == null) return View("Error");

            var journal = _journalService.GetById(entry.JournalId);
            if (journal == null) return HttpNotFound("Journal not found.");

            ViewBag.JournalId = entry.JournalId;
            ViewBag.JournalTitle = journal.Title;

            if (!ModelState.IsValid)
                return View(entry);

            // Admin creeaza entry pt jurnal -> owner = user-ul jurnalului
            var ownerUserId = journal.UserId;

            entry.CreatedAt = entry.CreatedAt == default(DateTime) ? DateTime.Now : entry.CreatedAt;
            entry.UpdatedAt = DateTime.Now;
            entry.IsDeleted = false;

            _entryService.Create(entry, ownerUserId);

            return RedirectToAction("Index", new { journalId = entry.JournalId });
        }

        // GET: Admin/Entries/Edit/5
        public ActionResult Edit(int id)
        {
            var entry = _entryService.GetById(id);
            if (entry == null) return HttpNotFound("Entry not found.");

            ViewBag.JournalId = entry.JournalId;
            return View(entry);
        }

        // POST: Admin/Entries/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Entry entry)
        {
            if (entry == null) return View("Error");

            if (!ModelState.IsValid)
            {
                ViewBag.JournalId = entry.JournalId;
                return View(entry);
            }

            // pastram CreatedAt din DB (ca sa nu il resetam din greseala)
            var dbEntry = _entryService.GetById(entry.EntryId);
            if (dbEntry == null) return HttpNotFound("Entry not found.");

            dbEntry.Title = entry.Title;
            dbEntry.Content = entry.Content;
            dbEntry.Location = entry.Location;
            dbEntry.UpdatedAt = DateTime.Now;

            _entryService.Update(dbEntry);

            return RedirectToAction("Index", new { journalId = dbEntry.JournalId });
        }

        // GET: Admin/Entries/Delete/5
        public ActionResult Delete(int id)
        {
            var entry = _entryService.GetById(id);
            if (entry == null) return HttpNotFound("Entry not found.");

            ViewBag.JournalId = entry.JournalId;
            return View(entry);
        }

        // POST: Admin/Entries/Delete/5
        // (pattern standard MVC: ActionName("Delete") ca view-ul de obicei posteaza pe "Delete")
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int journalId)
        {
            _entryService.Delete(id); // soft delete (conform lab)
            return RedirectToAction("Index", new { journalId });
        }

        // GET: Admin/Entries/Deleted?journalId=1
        public ActionResult Deleted(int? journalId)
        {
            if (!journalId.HasValue || journalId.Value <= 0)
                return RedirectToAction("Index", "Journals");

            var journal = _journalService.GetById(journalId.Value);
            if (journal == null)
                return HttpNotFound("Journal not found.");

            var entries = _entryService.GetDeletedByJournal(journalId.Value)
                ?.ToList() ?? Enumerable.Empty<Entry>();

            ViewBag.JournalId = journalId.Value;
            ViewBag.JournalTitle = journal.Title;

            return View(entries);
        }

        // POST: Admin/Entries/Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(int id, int journalId)
        {
            _entryService.Restore(id);
            return RedirectToAction("Index", new { journalId });
        }
    }
}
