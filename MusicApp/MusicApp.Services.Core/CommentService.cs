using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Comment;

namespace MusicApp.Services.Core
{
    public class CommentService : ICommentService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        public CommentService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task AddCommentAsync(PostCommentInputModel inputModel, string userId)
        {
            Comment comment = new Comment()
            {
                Id = Guid.NewGuid(),
                SongId = inputModel.SongId,
                Text = inputModel.Text,
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            };
            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<CommentViewModel>> GetCommentsAsync(Guid songId, string userId)
        {
            var comments = await dbContext
                .Comments
                .Where(c => c.SongId == songId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new CommentViewModel()
                {
                    Username = c.User.UserName!,
                    Text = c.Text,
                    CreatedOn = c.CreatedOn,
                    IsOwner=c.UserId == userId,
                    Id=c.Id,
                    UserId=userId
                })
                .ToListAsync();

            return comments;
        }
        public async Task<DeleteCommentViewModel?> GetCommentToDeleteAsync(string userId, Guid commentId)
        {
            DeleteCommentViewModel? commentToDelete = null;

            Comment? comment = await dbContext
                .Comments
                .Include(c => c.User)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == commentId);

            if (comment != null && comment.UserId.ToLower() == userId.ToLower())
            {
                commentToDelete = new DeleteCommentViewModel()
                {
                    Id = comment.Id,
                    PublisherId = comment.UserId,
                    SongId=comment.SongId
                };
            }
            return commentToDelete;
        }

        public async Task<bool> SoftDeleteCommentAsync(string userId, DeleteCommentViewModel viewModel)
        {
            var deletedComment = await dbContext.Comments.FindAsync(viewModel.Id);

            if (deletedComment != null &&
                deletedComment.UserId==viewModel.PublisherId)
            {
                deletedComment.IsDeleted = true;
                await dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
