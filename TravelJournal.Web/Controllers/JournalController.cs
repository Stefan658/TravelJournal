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


        // GET: /Journal/Details?id=1
        public ActionResult Details(int id)
        {
            var journal = _journalService.GetById(id);
            if (journal == null)
                return HttpNotFound();

            var vm = new JournalViewModel
            {
                JournalId = journal.JournalId,
                UserId = journal.UserId,
                Title = journal.Title,
                Description = journal.Description,
                CreatedAt = journal.CreatedAt,
                IsPublic = journal.IsPublic,
                EntryCount = journal.Entries?.Count(e => !e.IsDeleted) ?? 0
            };

            return View(vm); // ✅ CORECT
        }




        // GET: /Journal/Create?userId=5
        public ActionResult Create(int userId)
        {
            logger.Info($"Displaying Journal Create view for UserId={userId}");


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
            logger.Info($"Attempting to create journal for UserId={model.UserId}");

            if (!ModelState.IsValid)
            {
                logger.Warn("Journal creation failed — model invalid");
                return View(model);

            }

            try
            {

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

                logger.Info($"Journal created successfully with ID={journal.JournalId}");


                return RedirectToAction("Index", new { userId = model.UserId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error creating journal for UserId={model.UserId}");
                return View("Error");
            }

        }

        
        // GET: /Journal/Edit/5
        public ActionResult Edit(int id)
        {
            logger.Info($"Accessing Journal Edit for ID={id}");

            var journal = _journalService.GetById(id);
            if (journal == null)
            {
                logger.Warn($"Journal ID={id} not found for editing");
                return HttpNotFound();
            }



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
            logger.Info($"Attempting to update Journal ID={model.JournalId}");

            if (!ModelState.IsValid)
            {
                logger.Warn($"Journal update failed — invalid model for ID={model.JournalId}");
                return View(model);
            }


            try
            {
                var journal = _journalService.GetById(model.JournalId);
            if (journal == null)
            {
                logger.Warn($"Journal ID={model.JournalId} not found for update");
                return HttpNotFound();
            }

            journal.Title = model.Title;
            journal.Description = model.Description;
            journal.IsPublic = model.IsPublic;
            journal.UpdatedAt = DateTime.Now;

            _journalService.Update(journal);

            logger.Info($"Journal ID={model.JournalId} updated successfully");

            return RedirectToAction("Index", new { userId = model.UserId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error updating Journal ID={model.JournalId}");
                return View("Error");
            }
        }

       
        // GET: /Journal/Delete/5
        public ActionResult Delete(int id)
        {
            logger.Info($"Accessing Journal Delete confirmation for ID={id}");

            var journal = _journalService.GetById(id);
            if (journal == null)
            {
                logger.Warn($"Journal ID={id} not found for deletion");
                return HttpNotFound();
            }

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
            logger.Info($"Attempting to delete Journal ID={id}");


            try { 
            var journal = _journalService.GetById(id);
            

            if (journal == null)
            {
                logger.Warn($"Delete failed — Journal ID={id} not found");
                return HttpNotFound();
            }


            var userId = journal.UserId;

            _journalService.Delete(id);

            logger.Info($"Journal ID={id} deleted successfully");


            return RedirectToAction("Index", new { userId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error deleting Journal ID={id}");
                return View("Error");
            }
        }
    }
}
