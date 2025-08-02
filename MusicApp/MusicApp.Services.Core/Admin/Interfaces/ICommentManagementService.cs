using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Admin.CommentManagement;

namespace MusicApp.Services.Core.Admin.Interfaces
{
    public interface ICommentManagementService
    {
        Task<IEnumerable<CommentManagementViewModel>> GetAllCommentsAsync();
    }
}
