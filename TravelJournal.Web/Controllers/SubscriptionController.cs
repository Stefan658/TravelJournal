using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TravelJournal.Services.Interfaces;
using TravelJournal.Web.ViewModels.Subscriptions;

namespace TravelJournal.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUserService _userService;

        public SubscriptionController(ISubscriptionService subscriptionService, IUserService userService)
        {
            _subscriptionService = subscriptionService;
            _userService = userService;
        }

        // GET: /Subscription/Plans
        [Authorize]
        public ActionResult Plans()
        {
            var username = User.Identity.Name;
            var user = _userService.GetByUsername(username);
            if (user == null) return HttpNotFound();

            var plans = _subscriptionService.GetAll().ToList();

            var vm = new PlansViewModel
            {
                UserId = user.UserId,
                CurrentSubscriptionId = user.SubscriptionId,
                Plans = plans
            };

            return View(vm);
        }


        // GET: /Subscription/My
        [Authorize]
        public ActionResult My()
        {
            var username = User.Identity.Name;
            var user = _userService.GetByUsername(username);
            if (user == null) return HttpNotFound("User not found.");

            var plan = _subscriptionService.GetById(user.SubscriptionId);

            var vm = new MySubscriptionViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                CurrentPlanName = plan?.Name ?? "Unknown",
                CurrentPlanPrice = plan?.Price ?? 0,
                CanUploadMedia = _subscriptionService.CanUploadMedia(user.SubscriptionId),
                CanExportPdf = _subscriptionService.CanExportPdf(user.SubscriptionId),
                CanUseMap = _subscriptionService.CanUseMap(user.SubscriptionId),
            };

            return View(vm);
        }


        // POST: /Subscription/ChangePlan
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult ChangePlan(int planId)
        {
            var username = User.Identity.Name;
            var user = _userService.GetByUsername(username);
            if (user == null) return HttpNotFound();

            var plan = _subscriptionService.GetById(planId);
            if (plan == null || !plan.IsActive)
                return new HttpStatusCodeResult(400);

            user.SubscriptionId = planId;
            _userService.Update(user);

            TempData["Success"] = $"Plan changed to: {plan.Name}";
            return RedirectToAction("My");
        }

    }
}
