using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Data.Models.Enums;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Comment;

namespace MusicApp.Services.Core
{
    public class CommentService : ICommentService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly INotificationsService notificationService;
        public CommentService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager, INotificationsService notificationService)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.notificationService = notificationService;
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

            Song? song = await dbContext.Songs.FindAsync(inputModel.SongId);

            if (song != null && song.PublisherId != userId)
            {
                await notificationService.NotifyCommentAsync(song.Id, comment.Id, comment.Text, userId);
            }
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
                    IsOwner = c.UserId == userId,
                    Id = c.Id,
                    UserId = userId
                })
                .ToListAsync();

            return comments;
        }
        public async Task<DeleteCommentViewModel?> GetCommentToDeleteAsync(string userId, Guid commentId)
        {
            DeleteCommentViewModel? commentToDelete = null;
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            Comment? comment = await dbContext
                .Comments
                .Include(c => c.User)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == commentId);

            if (comment != null && user != null)
            {
                bool isUserAdmin = await userManager.IsInRoleAsync(user, "Admin");
                if (comment.UserId.ToLower() == userId.ToLower() || isUserAdmin)
                {
                    commentToDelete = new DeleteCommentViewModel()
                    {
                        Id = comment.Id,
                        PublisherId = comment.UserId,
                        SongId = comment.SongId
                    };
                }
            }
            return commentToDelete;
        }

        public async Task<bool> SoftDeleteCommentAsync(string userId, DeleteCommentViewModel viewModel)
        {
            var deletedComment = await dbContext.Comments.FindAsync(viewModel.Id);
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (deletedComment != null && user != null)
            {
                bool isUserAdmin = await userManager.IsInRoleAsync(user, "Admin");

                if (deletedComment.UserId == viewModel.PublisherId || isUserAdmin)
                {
                    deletedComment.IsDeleted = true;

                    IEnumerable<Notification> notificationsToRemove = dbContext
                        .Notifications
                        .Where(n => n.CommentId == viewModel.Id && n.Type == NotificationType.Comment);

                    dbContext.Notifications.RemoveRange(notificationsToRemove);

                    await dbContext.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task<int> GetCommentsCountAsync()
        {
            int comments = await dbContext
                .Comments
                .AsNoTracking()
                .Where(c=>c.IsDeleted==false)
                .CountAsync();

            return comments;
        }
    }
}
