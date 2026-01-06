using System;
using System.Linq;
using System.Web.Mvc;

using TravelJournal.Domain.Entities;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.ViewModels.Journals;

namespace TravelJournal.Web.Areas.Admin.Controllers
{
    public class JournalsController : Controller
    {
        private readonly IJournalService _journalService;

        // Temporar pana avem auth: folosim userId=1 (sau schimba dupa seed-ul tau)
        private const int DefaultUserId = 1;

        public JournalsController(IJournalService journalService)
        {
            _journalService = journalService;
        }

        // GET: Admin/Journals
        public ActionResult Index()
        {
            var journals = _journalService.GetAll()
                .Select(j => new JournalViewModel
                {
                    JournalId = j.JournalId,
                    UserId = j.UserId,
                    Title = j.Title,
                    Description = j.Description,
                    IsPublic = j.IsPublic,
                    CreatedAt = j.CreatedAt,
                    EntryCount = j.Entries != null ? j.Entries.Count : 0
                })
                .ToList();

            return View(journals);
        }

            // GET: Admin/Journals/Details/5
            public ActionResult Details(int id)
        {
            var j = _journalService.GetById(id);
            if (j == null) return HttpNotFound();

            var vm = new JournalViewModel
            {
                JournalId = j.JournalId,
                UserId = j.UserId,
                Title = j.Title,
                Description = j.Description,
                IsPublic = j.IsPublic,
                CreatedAt = j.CreatedAt,
                EntryCount = j.Entries != null ? j.Entries.Count : 0
            };

            return View(vm);
        }

        // GET: Admin/Journals/Create
        public ActionResult Create()
        {
            return View(new CreateJournalViewModel
            {
                UserId = DefaultUserId,
                IsPublic = true
            });
        }

        // POST: Admin/Journals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateJournalViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = new Journal
            {
                UserId = model.UserId != 0 ? model.UserId : DefaultUserId,
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _journalService.Create(entity);
            return RedirectToAction("Index");
        }

        // GET: Admin/Journals/Edit/5
        public ActionResult Edit(int id)
        {
            var j = _journalService.GetById(id);
            if (j == null) return HttpNotFound();

            var vm = new JournalViewModel
            {
                JournalId = j.JournalId,
                UserId = j.UserId,
                Title = j.Title,
                Description = j.Description,
                IsPublic = j.IsPublic,
                CreatedAt = j.CreatedAt,
                EntryCount = j.Entries != null ? j.Entries.Count : 0
            };

            return View(vm);
        }

        // POST: Admin/Journals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JournalViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = new Journal
            {
                JournalId = model.JournalId,
                UserId = model.UserId != 0 ? model.UserId : DefaultUserId,
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                // pastram CreatedAt (din model) ca sa nu-l pierdem
                CreatedAt = model.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _journalService.Update(entity);
            return RedirectToAction("Index");
        }

        // GET: Admin/Journals/Delete/5
        public ActionResult Delete(int id)
        {
            var j = _journalService.GetById(id);
            if (j == null) return HttpNotFound();

            var vm = new JournalViewModel
            {
                JournalId = j.JournalId,
                UserId = j.UserId,
                Title = j.Title,
                Description = j.Description,
                IsPublic = j.IsPublic,
                CreatedAt = j.CreatedAt,
                EntryCount = j.Entries != null ? j.Entries.Count : 0
            };

            return View(vm);
        }

        // POST: Admin/Journals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _journalService.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
