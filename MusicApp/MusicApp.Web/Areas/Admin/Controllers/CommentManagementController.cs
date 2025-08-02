using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core.Admin.Interfaces;
using MusicApp.Web.ViewModels.Admin.CommentManagement;

namespace MusicApp.Web.Areas.Admin.Controllers
{
    public class CommentManagementController : BaseAdminController
    {
        private readonly ICommentManagementService commentManagementService;

        public CommentManagementController(ICommentManagementService commentManagementService)
        {
            this.commentManagementService = commentManagementService;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<CommentManagementViewModel> comments=await commentManagementService
                .GetAllCommentsAsync();
            return View(comments);
        }
    }
}
