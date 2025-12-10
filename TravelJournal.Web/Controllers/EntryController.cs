using System;
using System.Linq;
using System.Web.Mvc;

using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.ViewModels.Entries;

namespace TravelJournal.Web.Controllers
{
    public class EntryController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly IJournalService _journalService;

        public EntryController(IEntryService entryService, IJournalService journalService)
        {
            //System.IO.File.WriteAllText(@"C:\entry-log.txt", "ENTRY CONSTRUCTOR HIT");


            _entryService = entryService;
            _journalService = journalService;
        }

        // GET: /Entry?journalId=5
        public ActionResult Index(int journalId)
        {
            var entries = _entryService.GetByJournal(journalId)
                                       .Select(e => MapToViewModel(e))
                                       .ToList();

            ViewBag.JournalId = journalId;
            return View(entries);
        }


        // GET: /Entry/Create?journalId=5
        public ActionResult Create(int journalId)
        {
            var journal = _journalService.GetById(journalId);

            var model = new CreateEntryViewModel
            {
                JournalId = journalId,
                UserId = journal.UserId
            };

            return View(model);
        }


        // POST: /Entry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEntryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entry = MapToEntity(model);
            _entryService.Create(entry, model.UserId);

            return RedirectToAction("Index", new { journalId = model.JournalId });
        }


        // GET: /Entry/Edit/10
        public ActionResult Edit(int id)
        {
            var entry = _entryService.GetById(id);
            var model = new CreateEntryViewModel
            {
                EntryId = entry.EntryId,
                JournalId = entry.JournalId,
                UserId = entry.UserId,
                Title = entry.Title,
                Content = entry.Content,
                Location = entry.Location
            };

            return View(model);
        }


        // POST: /Entry/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateEntryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entry = MapToEntity(model);
            _entryService.Update(entry);

            return RedirectToAction("Index", new { journalId = model.JournalId });
        }


        // GET: /Entry/Delete/10
        public ActionResult Delete(int id)
        {
            var entry = _entryService.GetById(id);
            var vm = MapToViewModel(entry);

            return View(vm);
        }


        // POST: /Entry/Delete/10
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entry = _entryService.GetById(id);
            _entryService.Delete(id);

            return RedirectToAction("Index", new { journalId = entry.JournalId });
        }

        // Mapping 
        private EntryViewModel MapToViewModel(Entry e)
        {
            return new EntryViewModel
            {
                EntryId = e.EntryId,
                JournalId = e.JournalId,
                UserId = e.UserId,
                Title = e.Title,
                Content = e.Content,
                Location = e.Location,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };
        }

        private Entry MapToEntity(CreateEntryViewModel model)
        {
            return new Entry
            {
                EntryId = model.EntryId,
                JournalId = model.JournalId,
                UserId = model.UserId,
                Title = model.Title,
                Content = model.Content,
                Location = model.Location,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }
}
