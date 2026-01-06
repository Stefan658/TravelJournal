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
                var entries = _entryService.GetByJournal(journalId)
                                           .Select(e => MapToViewModel(e))
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


        // GET: /Entry/Create?journalId=5
        public ActionResult Create(int journalId)
        {
            logger.Info($"Opening Create Entry page for JournalId={journalId}");

            var journal = _journalService.GetById(journalId);
            if (journal == null)
            {
                logger.Warn($"JournalId={journalId} not found when accessing Create Entry");
                return HttpNotFound();
            }

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
            logger.Info($"Attempting to create entry for JournalId={model.JournalId}");

            if (!ModelState.IsValid)
            {
                logger.Warn("Entry create failed due to invalid model");
                return View(model);
            }

            try
            {
                var entry = MapToEntity(model);
                _entryService.Create(entry, model.UserId);

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

            var entry = _entryService.GetById(id);
            if (entry == null)
            {
                logger.Warn($"EntryId={id} not found during Edit GET");
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
                var existing = _entryService.GetById(model.EntryId);
                if (existing == null)
                {
                    logger.Warn($"EntryId={model.EntryId} not found for update");
                    return HttpNotFound();
                }

                var entry = MapToEntity(model);
                _entryService.Update(entry);

                logger.Info($"EntryId={model.EntryId} updated successfully");

                return RedirectToAction("Index", new { journalId = model.JournalId });
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

            var entry = _entryService.GetById(id);
            if (entry == null)
            {
                logger.Warn($"EntryId={id} not found for deletion");
                return HttpNotFound();
            }

            var vm = MapToViewModel(entry);
            return View(vm);
        }


        // POST: /Entry/Delete/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            logger.Info($"Attempting to delete EntryId={id}");

            try
            {
                var entry = _entryService.GetById(id);
                if (entry == null)
                {
                    logger.Warn($"EntryId={id} not found during deletion");
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

        // GET: /Entry/Details?id=1
        public ActionResult Details(int id)
        {
            logger.Info($"Accessing Entry Details for EntryId={id}");

            var entry = _entryService.GetById(id);
            if (entry == null)
            {
                logger.Warn($"EntryId={id} not found during Details");
                return HttpNotFound();
            }

            var vm = MapToViewModel(entry);
            return View(vm);
        }



    }
}
