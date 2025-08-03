using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Admin;
using NUnit.Framework;

namespace MusicApp.Services.Tests.Services.Admin
{
    [TestFixture]
    public class SongManagementServiceTests
    {
        private MusicAppDbContext dbContext;
        private SongManagementService service;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            dbContext = new MusicAppDbContext(options);

            var genre = new Genre { Id = 1, Name = "Pop" };
            var user = new ApplicationUser { Id = "user123", UserName = "testuser", Email = "test@example.com" };

            await dbContext.Genres.AddAsync(genre);
            await dbContext.Users.AddAsync(user);

            var song1 = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song 1",
                Artist = "Artist 1",
                AudioUrl = "https://example.com/audio1.mp3",
                ReleaseDate = new DateTime(2020, 1, 1),
                PublisherId = user.Id,
                Publisher = user,
                GenreId = genre.Id,
                Genre = genre,
                IsDeleted = false
            };

            var song2 = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song 2",
                Artist = "Artist 2",
                AudioUrl = "https://example.com/audio2.mp3",
                ReleaseDate = new DateTime(2021, 1, 1),
                PublisherId = user.Id,
                Publisher = user,
                GenreId = genre.Id,
                Genre = genre,
                IsDeleted = true
            };

            await dbContext.Songs.AddRangeAsync(song1, song2);
            await dbContext.SaveChangesAsync();

            service = new SongManagementService(dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [Test]
        public async Task GetAllSongsAsync_ReturnsAllSongsIncludingDeleted()
        {
            var result = await service.GetAllSongsAsync(null);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetAllSongsAsync_WithSearchTerm_FiltersCorrectly()
        {
            var result = await service.GetAllSongsAsync("Song 1");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Test Song 1", result.First().Title);
        }

        [Test]
        public async Task SoftDeleteAsync_SetsIsDeletedToTrue()
        {
            var song = dbContext.Songs.First(s => !s.IsDeleted);
            await service.SoftDeleteAsync(song.Id);

            var updated = await dbContext.Songs.FindAsync(song.Id);
            Assert.IsTrue(updated!.IsDeleted);
        }

        [Test]
        public async Task RestoreAsync_SetsIsDeletedToFalse()
        {
            var song = dbContext.Songs.IgnoreQueryFilters().First(s => s.IsDeleted);
            await service.RestoreAsync(song.Id);

            var updated = await dbContext.Songs.IgnoreQueryFilters().FirstAsync(s => s.Id == song.Id);
            Assert.IsFalse(updated.IsDeleted);
        }
    }
}
