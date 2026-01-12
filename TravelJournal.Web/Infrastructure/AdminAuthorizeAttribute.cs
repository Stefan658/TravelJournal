using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TravelJournal.Web.Infrastructure
{
  

    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            Roles = "Admin";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // user logat, dar nu admin
                filterContext.Result = new HttpStatusCodeResult(403);
            }
            else
            {
                // neautentificat
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }

}