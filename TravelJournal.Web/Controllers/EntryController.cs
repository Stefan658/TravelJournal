using System;
using System.Linq;
using System.Web.Mvc;
using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.ViewModels.Entries;
using NLog;

namespace TravelJournal.Web.Controllers
{
    public class EntryController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly IJournalService _journalService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Temporar până la auth/seed demo (în sprintul Seed+Flow îl înlocuim cu userul autentificat)
        private const int CurrentUserId = 1;

        public EntryController(IEntryService entryService, IJournalService journalService)
        {
            _entryService = entryService;
            _journalService = journalService;
        }

        // GET: /Entry?journalId=5
        public ActionResult Index(int journalId)
        {
            logger.Info($"Accessing Entries/Index for JournalId={journalId}");

            try
            {
                // Ownership guard: userul curent trebuie să dețină jurnalul
                var journal = _journalService.GetByIdForUser(journalId, CurrentUserId);
                if (journal == null)
                {
                    logger.Warn($"JournalId={journalId} not found or not owned by CurrentUserId={CurrentUserId}");
                    return HttpNotFound();
                }

                var entries = _entryService.GetByJournal(journalId)
                                           .Where(e => !e.IsDeleted && e.UserId == CurrentUserId)
                                           .Select(MapToViewModel)
                                           .ToList();

                logger.Info($"Loaded {entries.Count} entries for JournalId={journalId}");

                ViewBag.JournalId = journalId;
                return View(entries);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error loading entries for JournalId={journalId}");
                return View("Error");
            }
        }

        // GET: /Entry/Details?id=1
        public ActionResult Details(int id)
        {
            logger.Info($"Accessing Entry Details for EntryId={id}");

            var entry = _entryService.GetByIdForUser(id, CurrentUserId);
            if (entry == null)
            {
                logger.Warn($"EntryId={id} not found or not owned by CurrentUserId={CurrentUserId} during Details");
                return HttpNotFound();
            }

            return View(MapToViewModel(entry));
        }

        // GET: /Entry/Create?journalId=5
        public ActionResult Create(int journalId)
        {
            logger.Info($"Opening Create Entry page for JournalId={journalId}");

            // Ownership guard: userul curent trebuie să dețină jurnalul
            var journal = _journalService.GetByIdForUser(journalId, CurrentUserId);
            if (journal == null)
            {
                logger.Warn($"JournalId={journalId} not found or not owned by CurrentUserId={CurrentUserId} when accessing Create Entry");
                return HttpNotFound();
            }

            var model = new CreateEntryViewModel
            {
                JournalId = journalId,
                // NU ne bazăm pe UserId din form, dar îl putem popula pentru afișare (dacă view-ul îl are)
                UserId = CurrentUserId
            };

            return View(model);
        }

        // POST: /Entry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEntryViewModel model)
        {
            logger.Info($"Attempting to create entry for JournalId={model.JournalId}");

            if (!ModelState.IsValid)
            {
                logger.Warn("Entry create failed due to invalid model");
                return View(model);
            }

            try
            {
                // Ownership guard: userul curent trebuie să dețină jurnalul
                var journal = _journalService.GetByIdForUser(model.JournalId, CurrentUserId);
                if (journal == null)
                {
                    logger.Warn($"Create Entry blocked: JournalId={model.JournalId} not owned by CurrentUserId={CurrentUserId}");
                    return HttpNotFound();
                }

                // Anti-tamper: userId se ia din context (CurrentUserId), nu din model
                var entry = MapToEntityForCreate(model, CurrentUserId);

                _entryService.Create(entry, CurrentUserId);

                logger.Info($"Entry created successfully with ID={entry.EntryId}");
                return RedirectToAction("Index", new { journalId = model.JournalId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error creating entry for JournalId={model.JournalId}");
                return View("Error");
            }
        }

        // GET: /Entry/Edit/10
        public ActionResult Edit(int id)
        {
            logger.Info($"Accessing Entry Edit page for EntryId={id}");

            var entry = _entryService.GetByIdForUser(id, CurrentUserId);
            if (entry == null)
            {
                logger.Warn($"EntryId={id} not found or not owned by CurrentUserId={CurrentUserId} during Edit GET");
                return HttpNotFound();
            }

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
            logger.Info($"Attempting to update EntryId={model.EntryId}");

            if (!ModelState.IsValid)
            {
                logger.Warn($"Entry update failed - invalid model for EntryId={model.EntryId}");
                return View(model);
            }

            try
            {
                // Ownership guard: entry trebuie să fie al userului curent
                var existing = _entryService.GetByIdForUser(model.EntryId, CurrentUserId);
                if (existing == null)
                {
                    logger.Warn($"EntryId={model.EntryId} not found or not owned by CurrentUserId={CurrentUserId} for update");
                    return HttpNotFound();
                }

                // Anti-tamper: JournalId/UserId nu se iau din model, ci rămân din existing
                existing.Title = model.Title;
                existing.Content = model.Content;
                existing.Location = model.Location;
                existing.UpdatedAt = DateTime.Now;

                _entryService.Update(existing);

                logger.Info($"EntryId={model.EntryId} updated successfully");
                return RedirectToAction("Index", new { journalId = existing.JournalId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error updating EntryId={model.EntryId}");
                return View("Error");
            }
        }

        // GET: /Entry/Delete/10
        public ActionResult Delete(int id)
        {
            logger.Info($"Opening Delete confirmation for EntryId={id}");

            var entry = _entryService.GetByIdForUser(id, CurrentUserId);
            if (entry == null)
            {
                logger.Warn($"EntryId={id} not found or not owned by CurrentUserId={CurrentUserId} for deletion");
                return HttpNotFound();
            }

            return View(MapToViewModel(entry));
        }

        // POST: /Entry/Delete/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            logger.Info($"Attempting to delete EntryId={id}");

            try
            {
                var entry = _entryService.GetByIdForUser(id, CurrentUserId);
                if (entry == null)
                {
                    logger.Warn($"EntryId={id} not found or not owned by CurrentUserId={CurrentUserId} during deletion");
                    return HttpNotFound();
                }

                var journalId = entry.JournalId;

                _entryService.Delete(id);

                logger.Info($"EntryId={id} deleted successfully");
                return RedirectToAction("Index", new { journalId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error deleting EntryId={id}");
                return View("Error");
            }
        }

        // -----------------------
        // Mapping helpers
        // -----------------------

        private static EntryViewModel MapToViewModel(Entry e)
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

        private static Entry MapToEntityForCreate(CreateEntryViewModel model, int userId)
        {
            return new Entry
            {
                // EntryId va fi setat de DB
                JournalId = model.JournalId,
                UserId = userId, // anti-tamper
                Title = model.Title,
                Content = model.Content,
                Location = model.Location,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }
}
