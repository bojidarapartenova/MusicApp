using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MusicApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public abstract class BaseAdminController : Controller
    {
        private bool IsUserAuthenticated()
        {
            bool retRes = false;
            if (this.User.Identity != null)
            {
                retRes = this.User.Identity.IsAuthenticated;
            }

            return retRes;
        }

        protected string? GetUserId()
        {
            string? userId = null;
            if (this.IsUserAuthenticated())
            {
                userId = this.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);
            }

            return userId;
        }
    }
}
