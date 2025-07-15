using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Comment;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Services.Core.Interfaces
{
    public interface ICommentService
    {
        Task AddCommentAsync(PostCommentInputModel inputModel, string userId);
        Task<IEnumerable<CommentViewModel>> GetCommentsAsync(Guid songId, string userId);
        Task<DeleteCommentViewModel?> GetCommentToDeleteAsync(string userId, Guid commentId);
        Task<bool> SoftDeleteCommentAsync(string userId, DeleteCommentViewModel viewModel);
    }
}
