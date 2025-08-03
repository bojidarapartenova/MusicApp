using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Admin;
using MusicApp.Web.ViewModels.Admin.CommentManagement;
using MusicApp.Data.Models;

namespace MusicApp.Services.Core.Admin.Tests
{
    [TestFixture]
    public class CommentManagementServiceTests
    {
        private DbContextOptions<MusicAppDbContext> _options;
        private MusicAppDbContext dbContext;
        private CommentManagementService service;

        private string userId1 = "user1";
        private string userId2 = "user2";
        private Guid commentId1;
        private Guid commentId2;

        [SetUp]
        public void SetUp()
        {
            // Use a unique DB per test run to avoid key conflicts
            _options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            dbContext = new MusicAppDbContext(_options);
            dbContext.Database.EnsureCreated();

            // Seed Users
            var user1 = new ApplicationUser { Id = userId1, UserName = "Alice" };
            var user2 = new ApplicationUser { Id = userId2, UserName = "Bob" };
            var publisher = new ApplicationUser { Id = "publisher1", UserName = "Publisher" };
            dbContext.Users.AddRange(user1, user2, publisher);

            // Seed Genre
            // Seed Genre (without fixed ID)
            var genre = new Genre
            {
                Name = "Rock"
            };
            dbContext.Genres.Add(genre);
            dbContext.SaveChanges(); // Needed to generate Genre.Id

            // Seed Song
            var song = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song",
                PublisherId = publisher.Id,
                Publisher = publisher,
                GenreId = genre.Id, // Now use the generated ID
                Genre = genre,
                Artist = "Test Artist",
                Duration = 180,
                ReleaseDate = DateTime.UtcNow,
                Likes = 0,
                ImageUrl = "http://example.com/image.jpg",
                AudioUrl = "http://example.com/audio.mp3"
            };

            // Seed Comments
            var now = DateTime.UtcNow;
            commentId1 = Guid.NewGuid();
            commentId2 = Guid.NewGuid();

            var comment1 = new Comment
            {
                Id = commentId1,
                Text = "First comment",
                UserId = userId1,
                User = user1,
                SongId = song.Id,
                Song = song,
                CreatedOn = now.AddMinutes(-10),
                IsDeleted = false
            };
            var comment2 = new Comment
            {
                Id = commentId2,
                Text = "Second comment",
                UserId = userId2,
                User = user2,
                SongId = song.Id,
                Song = song,
                CreatedOn = now.AddMinutes(-5),
                IsDeleted = false
            };

            dbContext.Comments.AddRange(comment1, comment2);
            dbContext.SaveChanges();

            service = new CommentManagementService(dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Dispose();
        }

        [Test]
        public async Task GetAllCommentsAsync_ReturnsAllCommentsOrderedByCreatedOnDescending()
        {
            var result = (await service.GetAllCommentsAsync()).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0].CreatedOn >= result[1].CreatedOn);
            Assert.AreEqual("Second comment", result[0].Text);
            Assert.AreEqual("Bob", result[0].UserName);
            Assert.AreEqual("First comment", result[1].Text);
            Assert.AreEqual("Alice", result[1].UserName);
        }

        [Test]
        public async Task DeleteCommentAsync_SetsIsDeletedToTrue()
        {
            await service.DeleteCommentAsync(commentId1);

            var updatedComment = dbContext.Comments.First(c => c.Id == commentId1);
            Assert.IsTrue(updatedComment.IsDeleted);
        }

        [Test]
        public async Task DeleteCommentAsync_NonExistingComment_DoesNothing()
        {
            var initialCount = dbContext.Comments.Count();

            await service.DeleteCommentAsync(Guid.NewGuid());

            Assert.AreEqual(initialCount, dbContext.Comments.Count());
            Assert.IsFalse(dbContext.Comments.Any(c => c.IsDeleted));
        }

        [Test]
        public async Task GetAllCommentsAsync_ReflectsDeletedComments()
        {
            await service.DeleteCommentAsync(commentId1);

            var results = (await service.GetAllCommentsAsync()).ToList();
            var deletedComment = results.FirstOrDefault(c => c.Id == commentId1);
            Assert.IsNotNull(deletedComment);
            Assert.IsTrue(deletedComment.IsDeleted);
        }
    }
}
