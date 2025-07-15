using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Comment;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Services.Core.Interfaces
{
    public interface ISongService
    {
        Task<IEnumerable<SongViewModel>> GetAllSongsAsync();
        Task <bool> AddSongAsync(string userId, AddSongInputModel inputModel);
        Task<EditSongInputModel?> GetSongToEditAsync(string userId, string? songId);
        Task<bool> EditSongAsync(EditSongInputModel inputModel);
        Task<DeleteSongViewModel?> GetSongToDeleteAsync(string userId, string? songId);
        Task<bool> SoftDeleteSongAsync(string userId, DeleteSongViewModel viewModel);
        Task<SongViewModel?> GetSongByIdAsync(string songId);
        Task AddCommentAsync(PostCommentInputModel inputModel, string userId);
        Task<IEnumerable<CommentViewModel>> GetCommentsAsync(Guid songId);
    }
}
