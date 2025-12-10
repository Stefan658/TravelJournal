using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TravelJournal.Services.Interfaces;
using TravelJournal.Domain.Entities;
using TravelJournal.Web.ViewModels.Journals;
using NLog;


namespace TravelJournal.Web.Controllers
{
    public class JournalController : Controller
    {
        private readonly IJournalService _journalService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        public JournalController(IJournalService journalService)
        {
            _journalService = journalService;
        }

        // GET: /Journal?userId=5
        public ActionResult Index(int userId)
        {
            logger.Info("Accessing Journals/Index");

            try
            {
                var journals = _journalService.GetByUser(userId);

                var model = journals.Select(j => new JournalViewModel
                {
                    JournalId = j.JournalId,
                    Title = j.Title,
                    Description = j.Description,
                    IsPublic = j.IsPublic,
                    CreatedAt = j.CreatedAt,
                    EntryCount = j.Entries != null ? j.Entries.Count : 0
                }).ToList();

                logger.Info($"Loaded {journals.Count()} journals from database");

                ViewBag.UserId = userId;
                return View(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while loading journals list");
                return View("Error");
            }
        }


        // GET: /Journal/Create?userId=5
        public ActionResult Create(int userId)
        {
            var model = new CreateJournalViewModel
            {
                UserId = userId,
                IsPublic = true
            };

            return View(model);
        }

        // POST: /Journal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateJournalViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var journal = new Journal
            {
                UserId = model.UserId,
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _journalService.Create(journal);

            return RedirectToAction("Index", new { userId = model.UserId });
        }

        // GET: /Journal/Edit/5
        public ActionResult Edit(int id)
        {
            var journal = _journalService.GetById(id);
            if (journal == null)
                return HttpNotFound();

            var model = new CreateJournalViewModel
            {
                JournalId = journal.JournalId,
                UserId = journal.UserId,
                Title = journal.Title,
                Description = journal.Description,
                IsPublic = journal.IsPublic
            };

            return View(model);
        }

        // POST: /Journal/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateJournalViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var journal = _journalService.GetById(model.JournalId);
            if (journal == null)
                return HttpNotFound();

            journal.Title = model.Title;
            journal.Description = model.Description;
            journal.IsPublic = model.IsPublic;
            journal.UpdatedAt = DateTime.Now;

            _journalService.Update(journal);

            return RedirectToAction("Index", new { userId = model.UserId });
        }

        // GET: /Journal/Delete/5
        public ActionResult Delete(int id)
        {
            var journal = _journalService.GetById(id);
            if (journal == null)
                return HttpNotFound();

            var model = new JournalViewModel
            {
                JournalId = journal.JournalId,
                Title = journal.Title,
                Description = journal.Description,
                IsPublic = journal.IsPublic,
                CreatedAt = journal.CreatedAt,
                EntryCount = journal.Entries != null ? journal.Entries.Count : 0
            };

            return View(model);
        }

        // POST: /Journal/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var journal = _journalService.GetById(id);
            if (journal == null)
                return HttpNotFound();


            var userId = journal.UserId;

            _journalService.Delete(id);

            return RedirectToAction("Index", new { userId });
        }
    }
}
