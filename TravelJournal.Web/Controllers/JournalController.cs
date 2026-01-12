using System;
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

        // Temporar până la auth/seed demo (în sprintul Seed+Flow îl înlocuim cu userul autentificat)
        private const int CurrentUserId = 1;

        public JournalController(IJournalService journalService)
        {
            _journalService = journalService;
        }

        // GET: /Journal
        public ActionResult Index()
        {
            logger.Info("Accessing Journals/Index");

            try
            {
                var journals = _journalService.GetByUser(CurrentUserId);

                var model = journals.Select(j => new JournalViewModel
                {
                    JournalId = j.JournalId,
                    UserId = j.UserId,
                    Title = j.Title,
                    Description = j.Description,
                    IsPublic = j.IsPublic,
                    CreatedAt = j.CreatedAt,
                    EntryCount = j.Entries != null ? j.Entries.Count(e => !e.IsDeleted) : 0
                }).ToList();

                logger.Info($"Loaded {model.Count} journals from database for CurrentUserId={CurrentUserId}");

                // pentru link-uri în view (dacă le folosești)
                ViewBag.UserId = CurrentUserId;

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
            logger.Info($"Accessing Journal Details for JournalId={id}");

            var journal = _journalService.GetByIdForUser(id, CurrentUserId);
            if (journal == null)
            {
                logger.Warn($"JournalId={id} not found or not owned by CurrentUserId={CurrentUserId} during Details");
                return HttpNotFound();
            }

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

            return View(vm);
        }

        // GET: /Journal/Create
        public ActionResult Create()
        {
            logger.Info($"Displaying Journal Create view for CurrentUserId={CurrentUserId}");

            var model = new CreateJournalViewModel
            {
                UserId = CurrentUserId,
                IsPublic = true
            };

            return View(model);
        }

        // POST: /Journal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateJournalViewModel model)
        {
            logger.Info($"Attempting to create journal for CurrentUserId={CurrentUserId}");

            if (!ModelState.IsValid)
            {
                logger.Warn("Journal creation failed — model invalid");
                // Anti-tamper: repunem userId corect în model
                model.UserId = CurrentUserId;
                return View(model);
            }

            try
            {
                // Anti-tamper: userId din context, nu din model
                var journal = new Journal
                {
                    UserId = CurrentUserId,
                    Title = model.Title,
                    Description = model.Description,
                    IsPublic = model.IsPublic,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _journalService.Create(journal);

                logger.Info($"Journal created successfully with ID={journal.JournalId}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error creating journal for CurrentUserId={CurrentUserId}");
                return View("Error");
            }
        }

        // GET: /Journal/Edit/5
        public ActionResult Edit(int id)
        {
            logger.Info($"Accessing Journal Edit for JournalId={id}");

            var journal = _journalService.GetByIdForUser(id, CurrentUserId);
            if (journal == null)
            {
                logger.Warn($"JournalId={id} not found or not owned by CurrentUserId={CurrentUserId} for editing");
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
            logger.Info($"Attempting to update JournalId={model.JournalId} for CurrentUserId={CurrentUserId}");

            if (!ModelState.IsValid)
            {
                logger.Warn($"Journal update failed — invalid model for JournalId={model.JournalId}");
                // Anti-tamper: repunem userId corect
                model.UserId = CurrentUserId;
                return View(model);
            }

            try
            {
                var journal = _journalService.GetByIdForUser(model.JournalId, CurrentUserId);
                if (journal == null)
                {
                    logger.Warn($"JournalId={model.JournalId} not found or not owned by CurrentUserId={CurrentUserId} for update");
                    return HttpNotFound();
                }

                journal.Title = model.Title;
                journal.Description = model.Description;
                journal.IsPublic = model.IsPublic;
                journal.UpdatedAt = DateTime.Now;

                _journalService.Update(journal);

                logger.Info($"JournalId={model.JournalId} updated successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error updating JournalId={model.JournalId}");
                return View("Error");
            }
        }

        // GET: /Journal/Delete/5
        public ActionResult Delete(int id)
        {
            logger.Info($"Accessing Journal Delete confirmation for JournalId={id}");

            var journal = _journalService.GetByIdForUser(id, CurrentUserId);
            if (journal == null)
            {
                logger.Warn($"JournalId={id} not found or not owned by CurrentUserId={CurrentUserId} for deletion");
                return HttpNotFound();
            }

            var model = new JournalViewModel
            {
                JournalId = journal.JournalId,
                UserId = journal.UserId,
                Title = journal.Title,
                Description = journal.Description,
                IsPublic = journal.IsPublic,
                CreatedAt = journal.CreatedAt,
                EntryCount = journal.Entries != null ? journal.Entries.Count(e => !e.IsDeleted) : 0
            };

            return View(model);
        }

        // POST: /Journal/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            logger.Info($"Attempting to delete JournalId={id} for CurrentUserId={CurrentUserId}");

            try
            {
                var journal = _journalService.GetByIdForUser(id, CurrentUserId);
                if (journal == null)
                {
                    logger.Warn($"Delete failed — JournalId={id} not found or not owned by CurrentUserId={CurrentUserId}");
                    return HttpNotFound();
                }

                _journalService.Delete(id);

                logger.Info($"JournalId={id} deleted successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error deleting JournalId={id}");
                return View("Error");
            }
        }
    }
}
