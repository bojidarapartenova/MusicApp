using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core;
using MusicApp.Web.ViewModels.Playlists;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApp.Tests.Services
{
    [TestFixture]
    public class PlaylistsServiceTests
    {
        private MusicAppDbContext dbContext;
        private Mock<UserManager<ApplicationUser>> userManagerMock;
        private PlaylistsService playlistsService;

        private ApplicationUser testUser;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new MusicAppDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            playlistsService = new PlaylistsService(dbContext, userManagerMock.Object);

            testUser = new ApplicationUser { Id = "user1", UserName = "TestUser" };
            dbContext.Users.Add(testUser);

            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = "My Playlist",
                UserId = testUser.Id,
                IsDeleted = false,
                IsDefault = false,
                ImageUrl = "/images/sample.jpg",
                PlaylistsSongs = new List<PlaylistSong>()
            };

            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [Test]
        public async Task GetUserPlaylistsAsync_ReturnsPlaylistsAndTotalCount()
        {
            var (playlists, totalCount) = await playlistsService.GetUserPlaylistsAsync(testUser.Id);

            Assert.AreEqual(1, totalCount);
            Assert.AreEqual(1, playlists.Count());
            Assert.AreEqual("My Playlist", playlists.First().Title);
        }

        [Test]
        public async Task CreatePlaylistAsync_AddsPlaylist()
        {
            var viewModel = new CreatePlaylistViewModel
            {
                Title = "New Playlist",
                ImageUrl = null,
                SelectedSongsIds = new List<Guid>()
            };

            await playlistsService.CreatePlaylistAsync(viewModel, testUser.Id);

            var playlists = await dbContext.Playlists.ToListAsync();
            Assert.AreEqual(2, playlists.Count);
            Assert.IsTrue(playlists.Any(p => p.Title == "New Playlist"));
        }

        [Test]
        public async Task SoftDeletePlaylistAsync_SetsIsDeletedTrue()
        {
            var playlist = await dbContext.Playlists.FirstAsync();
            var viewModel = new DeletePlaylistViewModel { Id = playlist.Id, Title = playlist.Title };

            userManagerMock.Setup(u => u.FindByIdAsync(testUser.Id)).ReturnsAsync(testUser);

            bool result = await playlistsService.SoftDeletePlaylistAsync(testUser.Id, viewModel);

            Assert.IsTrue(result);
            var updated = await dbContext.Playlists.FindAsync(playlist.Id);
            Assert.IsTrue(updated.IsDeleted);
        }

        [Test]
        public async Task EnsureFavoritesPlaylistExistsAsync_CreatesIfMissing()
        {
            await playlistsService.EnsureFavoritesPlaylistExistsAsync(testUser.Id);

            bool exists = await dbContext.Playlists.AnyAsync(p => p.UserId == testUser.Id && p.IsDefault);
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task GetFavoritesPlaylistAsync_ReturnsFavorites()
        {
            var playlist = new Playlist
            {
                Title = "Favorites",
                UserId = testUser.Id,
                IsDefault = true,
                ImageUrl = "test.jpg"
            };
            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();

            var result = await playlistsService.GetFavoritesPlaylistAsync(testUser.Id);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsDefault);
        }
    }
}
