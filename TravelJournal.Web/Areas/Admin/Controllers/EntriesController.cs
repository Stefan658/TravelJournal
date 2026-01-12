using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        // temporar (până la auth)
        private const int DefaultUserId = 1;

        public EntriesController(
            IEntryService entryService,
            IJournalService journalService)
        {
            _entryService = entryService;
            _journalService = journalService;
        }

        // GET: Admin/Entries?journalId=1
        public ActionResult Index(int journalId)
        {
            var entries = _entryService.GetByJournal(journalId);
            ViewBag.JournalId = journalId;
            return View(entries);
        }

        // GET: Admin/Entries/Details/5
        public ActionResult Details(int id)
        {
            var entry = _entryService.GetById(id);
            if (entry == null)
                return HttpNotFound();

            return View(entry);
        }

        // GET: Admin/Entries/Create?journalId=1
        public ActionResult Create(int journalId)
        {
            ViewBag.JournalId = journalId;
            return View(new Entry { JournalId = journalId });
        }

        // POST: Admin/Entries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Entry entry)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.JournalId = entry.JournalId;
                return View(entry);
            }

            _entryService.Create(entry, DefaultUserId);
            return RedirectToAction("Index", new { journalId = entry.JournalId });
        }

        // GET: Admin/Entries/Edit/5
        public ActionResult Edit(int id)
        {
            var entry = _entryService.GetById(id);
            if (entry == null)
                return HttpNotFound();

            return View(entry);
        }

        // POST: Admin/Entries/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Entry entry)
        {
            if (!ModelState.IsValid)
                return View(entry);

            _entryService.Update(entry);
            return RedirectToAction("Index", new { journalId = entry.JournalId });
        }

        // GET: Admin/Entries/Delete/5
        public ActionResult Delete(int id)
        {
            var entry = _entryService.GetById(id);
            if (entry == null)
                return HttpNotFound();

            return View(entry);
        }

        // POST: Admin/Entries/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int journalId)
        {
            _entryService.Delete(id);
            return RedirectToAction("Index", new { journalId });
        }

        // GET: Admin/Entries/Deleted?journalId=1
        public ActionResult Deleted(int journalId)
        {
            var entries = _entryService.GetDeletedByJournal(journalId); // trebuie sa existe in service
            ViewBag.JournalId = journalId;
            return View(entries);
        }

        // POST: Admin/Entries/Restore/2?journalId=1  (sau cu form hidden journalId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(int id, int journalId)
        {
            _entryService.Restore(id); // trebuie sa existe in service
            return RedirectToAction("Index", new { journalId });
        }



    }
}
