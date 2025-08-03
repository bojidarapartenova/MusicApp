using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Data.Models.Enums;
using MusicApp.Services.Core;
using MusicApp.Web.ViewModels.Notification;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApp.Tests.Services
{
    [TestFixture]
    public class NotificationsServiceTests
    {
        private MusicAppDbContext dbContext;
        private NotificationsService notificationsService;

        private ApplicationUser user1;
        private ApplicationUser user2;
        private Song song;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new MusicAppDbContext(options);
            notificationsService = new NotificationsService(dbContext);

            user1 = new ApplicationUser { Id = "user1", UserName = "UserOne" };
            user2 = new ApplicationUser { Id = "user2", UserName = "UserTwo" };

            song = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song",
                Artist = "Test Artist",
                AudioUrl = "http://test-audio.com",
                ImageUrl = "http://image.jpg",
                PublisherId = user1.Id
            };

            dbContext.Users.AddRange(user1, user2);
            dbContext.Songs.Add(song);
            await dbContext.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [Test]
        public async Task NotifySongLikedAsync_CreatesNotification_WhenUserIsNotPublisher()
        {
            // Act
            await notificationsService.NotifySongLikedAsync(song.Id, user2.Id);

            // Assert
            var notification = await dbContext.Notifications.FirstOrDefaultAsync();
            Assert.IsNotNull(notification);
            Assert.AreEqual(NotificationType.Like, notification.Type);
            Assert.AreEqual(user2.Id, notification.AuthorId);
            Assert.AreEqual(song.PublisherId, notification.UserId);
        }

        [Test]
        public async Task NotifySongLikedAsync_DoesNotNotify_WhenUserIsPublisher()
        {
            // Act
            await notificationsService.NotifySongLikedAsync(song.Id, user1.Id);

            // Assert
            var notification = await dbContext.Notifications.FirstOrDefaultAsync();
            Assert.IsNull(notification);
        }

        [Test]
        public async Task NotifyCommentAsync_CreatesNotification_WhenUserIsNotPublisher()
        {
            var commentId = Guid.NewGuid();

            // Act
            await notificationsService.NotifyCommentAsync(song.Id, commentId, "A new comment", user2.Id);

            // Assert
            var notification = await dbContext.Notifications.FirstOrDefaultAsync();
            Assert.IsNotNull(notification);
            Assert.AreEqual(NotificationType.Comment, notification.Type);
            Assert.AreEqual(commentId, notification.CommentId);
            Assert.AreEqual("A new comment", notification.Message);
            Assert.AreEqual(user2.Id, notification.AuthorId);
            Assert.AreEqual(user1.Id, notification.UserId); // song publisher
        }

        [Test]
        public async Task GetAllNotificationsAsync_ReturnsCorrectNotifications_WithoutFilter()
        {
            await notificationsService.NotifySongLikedAsync(song.Id, user2.Id);
            await notificationsService.NotifyCommentAsync(song.Id, Guid.NewGuid(), "test", user2.Id);

            // Act
            var result = await notificationsService.GetAllNotificationsAsync(user1.Id);

            // Assert
            Assert.AreEqual(2, result.Count());
        }

        [TestCase("like", 1)]
        [TestCase("comment", 1)]
        [TestCase("unread", 2)]
        public async Task GetAllNotificationsAsync_FiltersCorrectly(string filter, int expectedCount)
        {
            await notificationsService.NotifySongLikedAsync(song.Id, user2.Id);
            await notificationsService.NotifyCommentAsync(song.Id, Guid.NewGuid(), "test", user2.Id);

            // Act
            var result = await notificationsService.GetAllNotificationsAsync(user1.Id, filter);

            // Assert
            Assert.AreEqual(expectedCount, result.Count());
        }

        [Test]
        public async Task MarkAsReadAsync_MarksNotificationAsRead()
        {
            await notificationsService.NotifySongLikedAsync(song.Id, user2.Id);
            var notification = await dbContext.Notifications.FirstAsync();

            // Act
            await notificationsService.MarkAsReadAsync(notification.Id);

            // Assert
            var updated = await dbContext.Notifications.FindAsync(notification.Id);
            Assert.IsTrue(updated.IsRead);
        }

        [Test]
        public async Task GetUnreadAsync_ReturnsOnlyUnreadCount()
        {
            await notificationsService.NotifySongLikedAsync(song.Id, user2.Id);
            await notificationsService.NotifyCommentAsync(song.Id, Guid.NewGuid(), "test", user2.Id);

            var notificationToMark = await dbContext.Notifications.FirstAsync();
            notificationToMark.IsRead = true;
            await dbContext.SaveChangesAsync();

            // Act
            var unread = await notificationsService.GetUnreadAsync(user1.Id);

            // Assert
            Assert.AreEqual(1, unread);
        }
    }
}
