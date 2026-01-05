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

        // GET: /Subscription/Plans?userId=1
        public ActionResult Plans(int userId = 1)
        {
            var plans = _subscriptionService.GetAll().ToList();
            var user = _userService.GetById(userId);

            var vm = new PlansViewModel
            {
                UserId = userId,
                CurrentSubscriptionId = user?.SubscriptionId ?? 0,
                Plans = plans
            };

            return View(vm);
        }

        // GET: /Subscription/My?userId=1
        public ActionResult My(int userId = 1)
        {
            var user = _userService.GetById(userId);
            if (user == null) return HttpNotFound("User not found.");

            var plan = _subscriptionService.GetById(user.SubscriptionId);

            var vm = new MySubscriptionViewModel
            {
                UserId = userId,
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
        public ActionResult ChangePlan(int userId, int planId)
        {
            var user = _userService.GetById(userId);
            if (user == null) return HttpNotFound("User not found.");

            var plan = _subscriptionService.GetById(planId);
            if (plan == null) return HttpNotFound("Plan not found.");
            if (!plan.IsActive) return new HttpStatusCodeResult(400, "Plan is inactive.");

            user.SubscriptionId = planId;
            _userService.Update(user);

            TempData["Success"] = $"Plan changed to: {plan.Name}";
            return RedirectToAction("My", new { userId });
        }
    }
}
