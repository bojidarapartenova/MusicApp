using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Data.Models.Enums;
using MusicApp.Services.Core;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Comment;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApp.Tests.Services
{
    [TestFixture]
    public class CommentServiceTests
    {
        private MusicAppDbContext dbContext;
        private Mock<UserManager<ApplicationUser>> userManagerMock;
        private Mock<INotificationsService> notificationServiceMock;
        private CommentService commentService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
                .Options;

            dbContext = new MusicAppDbContext(options);

            // Setup mocks for UserManager<ApplicationUser>
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            notificationServiceMock = new Mock<INotificationsService>();

            commentService = new CommentService(dbContext, userManagerMock.Object, notificationServiceMock.Object);

            SeedDatabase().Wait();
        }

        private async Task SeedDatabase()
        {
            var user1 = new ApplicationUser { Id = "user1", UserName = "UserOne" };
            var user2 = new ApplicationUser { Id = "user2", UserName = "UserTwo" };

            var song1 = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Song 1",
                Artist = "Artist 1",          // <-- Required
                AudioUrl = "http://audiourl", // <-- Required
                PublisherId = "user1"
            };

            dbContext.Users.AddRange(user1, user2);
            dbContext.Songs.Add(song1);

            dbContext.Comments.Add(new Comment
            {
                Id = Guid.NewGuid(),
                SongId = song1.Id,
                Text = "Existing Comment",
                UserId = "user2",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false,
                User = user2
            });

            await dbContext.SaveChangesAsync();
        }


        [TearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [Test]
        public async Task AddCommentAsync_AddsCommentAndSendsNotification_WhenPublisherIsDifferentUser()
        {
            // Arrange
            var song = await dbContext.Songs.FirstAsync();
            var inputModel = new PostCommentInputModel
            {
                SongId = song.Id,
                Text = "New Comment"
            };
            var commentingUserId = "user2"; // different from publisher "user1"

            // Act
            await commentService.AddCommentAsync(inputModel, commentingUserId);

            // Assert
            var addedComment = await dbContext.Comments
                .FirstOrDefaultAsync(c => c.Text == "New Comment" && c.UserId == commentingUserId);

            Assert.IsNotNull(addedComment);

            notificationServiceMock.Verify(n => n.NotifyCommentAsync(
                song.Id,
                addedComment.Id,
                "New Comment",
                commentingUserId), Times.Once);
        }

        [Test]
        public async Task AddCommentAsync_DoesNotSendNotification_WhenPublisherIsSameUser()
        {
            // Arrange
            var song = await dbContext.Songs.FirstAsync();
            var inputModel = new PostCommentInputModel
            {
                SongId = song.Id,
                Text = "Publisher Comment"
            };
            var commentingUserId = song.PublisherId; // same user as publisher

            // Act
            await commentService.AddCommentAsync(inputModel, commentingUserId);

            // Assert
            var addedComment = await dbContext.Comments
                .FirstOrDefaultAsync(c => c.Text == "Publisher Comment" && c.UserId == commentingUserId);

            Assert.IsNotNull(addedComment);

            notificationServiceMock.Verify(n => n.NotifyCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetCommentsAsync_ReturnsCommentsForSong_WithCorrectOwnership()
        {
            // Arrange
            var song = await dbContext.Songs.FirstAsync();
            var userId = "user2";

            // Act
            var comments = (await commentService.GetCommentsAsync(song.Id, userId)).ToList();

            // Assert
            Assert.AreEqual(1, comments.Count);
            var comment = comments[0];
            Assert.AreEqual("UserTwo", comment.Username);
            Assert.AreEqual("Existing Comment", comment.Text);
            Assert.IsTrue(comment.IsOwner);
        }

        [Test]
        public async Task GetCommentToDeleteAsync_ReturnsCommentToDelete_IfUserIsOwner()
        {
            // Arrange
            var comment = await dbContext.Comments.Include(c => c.User).FirstAsync();
            var userId = comment.UserId;

            userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(comment.User);

            userManagerMock.Setup(u => u.IsInRoleAsync(comment.User, "Admin"))
                .ReturnsAsync(false);

            // Act
            var result = await commentService.GetCommentToDeleteAsync(userId, comment.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(comment.Id, result.Id);
            Assert.AreEqual(comment.UserId, result.PublisherId);
        }

        [Test]
        public async Task GetCommentToDeleteAsync_ReturnsCommentToDelete_IfUserIsAdmin()
        {
            // Arrange
            var comment = await dbContext.Comments.Include(c => c.User).FirstAsync();
            var userId = "adminUser";

            var adminUser = new ApplicationUser { Id = userId, UserName = "AdminUser" };

            userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(adminUser);

            userManagerMock.Setup(u => u.IsInRoleAsync(adminUser, "Admin"))
                .ReturnsAsync(true);

            // Act
            var result = await commentService.GetCommentToDeleteAsync(userId, comment.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(comment.Id, result.Id);
            Assert.AreEqual(comment.UserId, result.PublisherId);
        }

        [Test]
        public async Task SoftDeleteCommentAsync_DeletesCommentAndNotifications_WhenUserIsOwner()
        {
            // Arrange
            var comment = await dbContext.Comments.FirstAsync();
            var userId = comment.UserId;

            userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(await dbContext.Users.FindAsync(userId));

            userManagerMock.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
                .ReturnsAsync(false);

            // Add notification linked to comment
            // Add notification linked to comment, with required fields
            dbContext.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                Type = NotificationType.Comment,
                AuthorId = "user1",  // dummy but required
                UserId = "user2",    // dummy but required
                CreatedAt = DateTime.UtcNow,
                Message = "Test notification",
                SongId = comment.SongId
            });

            await dbContext.SaveChangesAsync();

            var viewModel = new DeleteCommentViewModel
            {
                Id = comment.Id,
                PublisherId = comment.UserId,
                SongId = comment.SongId
            };

            // Act
            var result = await commentService.SoftDeleteCommentAsync(userId, viewModel);

            // Assert
            Assert.IsTrue(result);
            var deletedComment = await dbContext.Comments.FindAsync(comment.Id);
            Assert.IsTrue(deletedComment.IsDeleted);

            var notifications = dbContext.Notifications.Where(n => n.CommentId == comment.Id);
            Assert.IsEmpty(notifications);
        }

        [Test]
        public async Task SoftDeleteCommentAsync_ReturnsFalse_WhenUserNotOwnerOrAdmin()
        {
            // Arrange
            var comment = await dbContext.Comments.FirstAsync();
            var userId = "unauthorizedUser";

            var someUser = new ApplicationUser { Id = userId, UserName = "NotOwner" };
            dbContext.Users.Add(someUser);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(someUser);

            userManagerMock.Setup(u => u.IsInRoleAsync(someUser, "Admin"))
                .ReturnsAsync(false);

            var viewModel = new DeleteCommentViewModel
            {
                Id = comment.Id,
                PublisherId = userId, 
                SongId = comment.SongId
            };

            // Act
            var result = await commentService.SoftDeleteCommentAsync(userId, viewModel);

            // Assert
            Assert.IsFalse(result);
            var deletedComment = await dbContext.Comments.FindAsync(comment.Id);
            Assert.IsFalse(deletedComment.IsDeleted);
        }



        [Test]
        public async Task GetCommentsCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var initialCount = await commentService.GetCommentsCountAsync();

            // Add another comment (not deleted)
            dbContext.Comments.Add(new Comment
            {
                Id = Guid.NewGuid(),
                SongId = (await dbContext.Songs.FirstAsync()).Id,
                Text = "Another comment",
                UserId = "user1",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            });
            await dbContext.SaveChangesAsync();

            // Add deleted comment
            dbContext.Comments.Add(new Comment
            {
                Id = Guid.NewGuid(),
                SongId = (await dbContext.Songs.FirstAsync()).Id,
                Text = "Deleted comment",
                UserId = "user1",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = true
            });
            await dbContext.SaveChangesAsync();

            // Act
            var count = await commentService.GetCommentsCountAsync();

            // Assert
            Assert.AreEqual(initialCount + 1, count); // Only non-deleted comments counted
        }
    }
}
