using System;
using System.Linq;
using System.Web.Mvc;

using NLog;

using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.ViewModels.Journals;

namespace TravelJournal.Web.Controllers
{
    [Authorize]
    public class JournalController : Controller
    {
        private readonly IJournalService _journalService;
        private readonly IUserService _userService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public JournalController(IJournalService journalService, IUserService userService)
        {
            _journalService = journalService ?? throw new ArgumentNullException(nameof(journalService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private int GetCurrentUserId()
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return 0;

            var user = _userService.GetByUsername(username);
            return user?.UserId ?? 0;
        }

        // RouteConfig: "/users/{userId}/journals" => Journal/Index
        // IMPORTANT: ignoram userId din URL, folosim userul autentificat
        public ActionResult Index(int userId = 0)
        {
            logger.Info("[JournalController] Index");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                // DOAR jurnalele userului curent
                var journals = _journalService.GetByUser(currentUserId);

                var model = journals.Select(j => new JournalViewModel
                {
                    JournalId = j.JournalId,
                    UserId = j.UserId,
                    Title = j.Title,
                    Description = j.Description,
                    IsPublic = j.IsPublic,
                    CreatedAt = j.CreatedAt,
                    EntryCount = (j.Entries != null)
                        ? j.Entries.Count(e => !e.IsDeleted)
                        : 0
                }).ToList();

                ViewBag.UserId = currentUserId;
                return View(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[JournalController] Error while loading journals list");
                return View("Error");
            }
        }

        // RouteConfig: "/journals/{id}" => Journal/Details
        public ActionResult Details(int id)
        {
            logger.Info($"[JournalController] Details JournalId={id}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                // ownership enforced
                var journal = _journalService.GetByIdForUser(id, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                var vm = new JournalViewModel
                {
                    JournalId = journal.JournalId,
                    UserId = journal.UserId,
                    Title = journal.Title,
                    Description = journal.Description,
                    IsPublic = journal.IsPublic,
                    CreatedAt = journal.CreatedAt,
                    EntryCount = (journal.Entries != null)
                        ? journal.Entries.Count(e => !e.IsDeleted)
                        : 0
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalController] Error while loading journal details (JournalId={id})");
                return View("Error");
            }
        }

        // GET: /Journal/Create
        public ActionResult Create()
        {
            return View(new CreateJournalViewModel());
        }

        // POST: /Journal/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateJournalViewModel model)
        {
            logger.Info("[JournalController] Create POST");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                if (!ModelState.IsValid)
                    return View(model);

                var journal = new Journal
                {
                    Title = model.Title,
                    Description = model.Description,
                    IsPublic = model.IsPublic,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UserId = currentUserId
                };

                _journalService.Create(journal);

                return RedirectToAction("Index", new { userId = currentUserId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[JournalController] Error creating journal");
                return View("Error");
            }
        }

        // GET: /Journal/Edit?id=5
        public ActionResult Edit(int id)
        {
            logger.Info($"[JournalController] Edit GET JournalId={id}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                var journal = _journalService.GetByIdForUser(id, currentUserId);
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
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalController] Error editing journal (JournalId={id})");
                return View("Error");
            }
        }

        // POST: /Journal/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateJournalViewModel model)
        {
            logger.Info($"[JournalController] Edit POST JournalId={model.JournalId}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                if (!ModelState.IsValid)
                    return View(model);

                var journal = _journalService.GetByIdForUser(model.JournalId, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                journal.Title = model.Title;
                journal.Description = model.Description;
                journal.IsPublic = model.IsPublic;
                journal.UpdatedAt = DateTime.Now;

                _journalService.Update(journal);

                return RedirectToAction("Index", new { userId = currentUserId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalController] Error updating journal (JournalId={model.JournalId})");
                return View("Error");
            }
        }

        // GET: /Journal/Delete?id=5
        public ActionResult Delete(int id)
        {
            logger.Info($"[JournalController] Delete GET JournalId={id}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                var journal = _journalService.GetByIdForUser(id, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                var vm = new JournalViewModel
                {
                    JournalId = journal.JournalId,
                    Title = journal.Title,
                    Description = journal.Description
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalController] Error deleting journal (JournalId={id})");
                return View("Error");
            }
        }

        // POST: /Journal/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            logger.Info($"[JournalController] DeleteConfirmed JournalId={id}");

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0) return new HttpUnauthorizedResult();

                var journal = _journalService.GetByIdForUser(id, currentUserId);
                if (journal == null)
                    return HttpNotFound();

                _journalService.Delete(id);

                return RedirectToAction("Index", new { userId = currentUserId });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[JournalController] Error in DeleteConfirmed (JournalId={id})");
                return View("Error");
            }
        }
    }
}
