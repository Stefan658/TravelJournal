using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text.pdf;

using iTextSharp.text;

using NLog;

using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.Helpers;
using TravelJournal.Web.ViewModels.Entries;
using TravelJournal.Web.ViewModels.Photos;
using System.Collections.Generic;

namespace TravelJournal.Web.Controllers
{
    [Authorize]
    public class EntryController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly IJournalService _journalService;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;
        private readonly ISubscriptionService _subscriptionService;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EntryController(
            IEntryService entryService,
            IJournalService journalService,
            IPhotoService photoService,
            IUserService userService,
            ISubscriptionService subscriptionService)
        {
            _entryService = entryService ?? throw new ArgumentNullException(nameof(entryService));
            _journalService = journalService ?? throw new ArgumentNullException(nameof(journalService));
            _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        }

        private int GetCurrentUserId()
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return 0;

            var user = _userService.GetByUsername(username);
            return user?.UserId ?? 0;
        }

        // RouteConfig: "/journals/{journalId}/entries" => Entry/Index
        public ActionResult Index(int journalId)
        {
            logger.Info($"[EntryController] Index journalId={journalId}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                // ownership guard pe jurnal
                var journal = _journalService.GetByIdForUser(journalId, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                var entries = _entryService.GetByJournal(journalId)
                    .Where(e => !e.IsDeleted && e.UserId == currentUserId)
                    .Select(e => new EntryViewModel
                    {
                     EntryId = e.EntryId,
                     Title = e.Title,
                     CreatedAt = e.CreatedAt,
                     JournalId = e.JournalId
                    })
                     .ToList();

                ViewBag.JournalId = journalId;
                ViewBag.JournalTitle = journal.Title;

                return View(entries);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryController] Error loading entries list (journalId={journalId})");
                return View("Error");
            }
        }

        // RouteConfig: "/entries/{id}" => Entry/Details
        public ActionResult Details(int id)
        {
            logger.Info($"[EntryController] Details entryId={id}");

            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return new HttpUnauthorizedResult();

            // ownership guard: entry must belong to current user
            var entry = _entryService.GetByIdForUser(id, currentUserId);
            if (entry == null) return HttpNotFound();

            var sub = _userService.GetSubscription(currentUserId);

            var subId = sub?.SubscriptionId ?? 0;   // sau sub?.Id, vezi cum e la tine în entity

            var photos = _photoService.GetByEntry(id)?.ToList();

            var vm = new TravelJournal.Web.ViewModels.Entries.EntryViewModel
            {
                EntryId = entry.EntryId,
                Title = entry.Title,
                // completeaza cu ce ai in entity (exact acele proprietati existente):
                Content = entry.Content,
                Location = entry.Location,
              

                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt,
                JournalId = entry.JournalId,
                UserId = entry.UserId,

                Photos = photos,
                SubscriptionName = sub?.Name ?? "Free",

                CanUploadPhotos = _subscriptionService.CanUploadMedia(subId),
                CanExportPdf = _subscriptionService.CanExportPdf(subId)


            };

            return View(vm);
        }


        // RouteConfig: "/journals/{journalId}/entries/create" => Entry/Create
        public ActionResult Create(int journalId)
        {
            logger.Info($"[EntryController] Create GET journalId={journalId}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                var journal = _journalService.GetByIdForUser(journalId, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                var model = new CreateEntryViewModel
                {
                    JournalId = journalId,
                    UserId = currentUserId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryController] Error in Create GET (journalId={journalId})");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateEntryViewModel model)
        {
            logger.Info($"[EntryController] Create POST journalId={model?.JournalId}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                if (!ModelState.IsValid)
                    return View(model);

                // ownership guard pe jurnal
                var journal = _journalService.GetByIdForUser(model.JournalId, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                var entry = new Entry
                {
                    JournalId = model.JournalId,
                    Title = model.Title,
                    Content = model.Content,
                    Location = model.Location,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                    // Latitude/Longitude nu exista in CreateEntryViewModel (sunt comentate) => nu setam
                };

                // IMPORTANT: semnatura reala e Create(entry, userId)
                _entryService.Create(entry, currentUserId);

                return RedirectToAction("Index", new { journalId = model.JournalId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[EntryController] Error creating entry");
                TempData["Error"] = ex.Message;
                return View("Error");
            }
        }

        public ActionResult Edit(int id)
        {
            logger.Info($"[EntryController] Edit GET EntryId={id}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                var entry = _entryService.GetByIdForUser(id, currentUserId);
                if (entry == null)
                    return HttpNotFound();

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
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryController] Error in Edit GET (EntryId={id})");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateEntryViewModel model)
        {
            logger.Info($"[EntryController] Edit POST EntryId={model?.EntryId}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                if (!ModelState.IsValid)
                    return View(model);

                var entry = _entryService.GetByIdForUser(model.EntryId, currentUserId);
                if (entry == null)
                    return HttpNotFound();

                entry.Title = model.Title;
                entry.Content = model.Content;
                entry.Location = model.Location;
                entry.UpdatedAt = DateTime.Now;

                _entryService.Update(entry);

                return RedirectToAction("Index", new { journalId = entry.JournalId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryController] Error updating entry (EntryId={model?.EntryId})");
                return View("Error");
            }
        }

        public ActionResult Delete(int id)
        {
            logger.Info($"[EntryController] Delete(GET) entryId={id}");

            var currentUserId = GetCurrentUserId();
            if (currentUserId <= 0) return new HttpUnauthorizedResult();

            // ownership guard
            var entry = _entryService.GetByIdForUser(id, currentUserId);
            if (entry == null) return HttpNotFound();

            var sub = _userService.GetSubscription(currentUserId);
            var photos = _photoService.GetByEntry(id)?.ToList();

            var vm = new TravelJournal.Web.ViewModels.Entries.EntryViewModel
            {
                EntryId = entry.EntryId,
                Title = entry.Title,
                Content = entry.Content,
                Location = entry.Location,
               

                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt,
                JournalId = entry.JournalId,
                UserId = entry.UserId,

                Photos = photos,
                SubscriptionName = sub?.Name ?? "Free"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            logger.Info($"[EntryController] DeleteConfirmed EntryId={id}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                var entry = _entryService.GetByIdForUser(id, currentUserId);
                if (entry == null)
                    return HttpNotFound();

                _entryService.Delete(id);

                return RedirectToAction("Index", new { journalId = entry.JournalId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryController] Error in DeleteConfirmed (EntryId={id})");
                return View("Error");
            }
        }

        // Trash (deleted entries) - daca ai view pentru asta
        public ActionResult Deleted(int journalId)
        {
            logger.Info($"[EntryController] Deleted journalId={journalId}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                var journal = _journalService.GetByIdForUser(journalId, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                var deleted = _entryService.GetDeletedByJournal(journalId)
                    .Where(e => e.UserId == currentUserId)
                    .ToList();

                ViewBag.JournalId = journalId;
                return View(deleted);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryController] Error in Deleted (journalId={journalId})");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Restore(int id)
        {
            logger.Info($"[EntryController] Restore EntryId={id}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                // ca sa nu restauram entry al altuia, verificam ownership pe entry (inclusiv daca e deleted)
                var entry = _entryService.GetById(id);
                if (entry == null || entry.UserId != currentUserId)
                    return HttpNotFound();

                _entryService.Restore(id);
                return RedirectToAction("Index", new { journalId = entry.JournalId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[EntryController] Error in Restore (EntryId={id})");
                return View("Error");
            }
        }

        // Upload photo - foloseste exact PhotoStorage + PhotoService.Upload(Photo, userId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadPhoto(UploadPhotoViewModel vm)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please select a valid image file.";
                    return RedirectToAction("Details", new { id = vm.EntryId });
                }

                // ownership guard pe entry
                var entry = _entryService.GetByIdForUser(vm.EntryId, currentUserId);
                if (entry == null)
                    return HttpNotFound();

                // 1) salvare fizica
                var savedRelativePath = PhotoStorage.Save(vm.File);

                // 2) persist in DB
                var photo = new Photo
                {
                    EntryId = vm.EntryId,
                    FilePath = savedRelativePath
                };

                _photoService.Upload(photo, currentUserId);

                TempData["Success"] = "Photo uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id = vm.EntryId });
        }

       

    }
}
