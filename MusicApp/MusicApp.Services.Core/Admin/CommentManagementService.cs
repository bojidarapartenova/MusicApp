using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Admin.Interfaces;
using MusicApp.Web.ViewModels.Admin.CommentManagement;
using MusicApp.Data.Models;

namespace MusicApp.Services.Core.Admin
{
    public class CommentManagementService: ICommentManagementService
    {
        private readonly MusicAppDbContext dbContext;

        public CommentManagementService(MusicAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<CommentManagementViewModel>> GetAllCommentsAsync()
        {
            IEnumerable<CommentManagementViewModel> comments = await dbContext
                .Comments
                .AsNoTracking()
                .OrderByDescending(c=>c.CreatedOn)
                .Select(c => new CommentManagementViewModel()
                {
                    Id = c.Id,
                    Text = c.Text,
                    UserName = c.User.UserName,
                    CreatedOn = c.CreatedOn,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            return comments;
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            Comment? comment = await dbContext
                .Comments
                .FirstAsync(c => c.Id == commentId);

            if(comment!=null)
            {
                comment.IsDeleted = true;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
