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

            // Add test user
            testUser = new ApplicationUser { Id = "user1", UserName = "TestUser" };
            await dbContext.Users.AddAsync(testUser);

            // Add genre (required for songs)
            var genre = new Genre { Id = 1, Name = "Pop" };
            await dbContext.Genres.AddAsync(genre);

            // Add songs
            var song1 = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Song 1",
                Artist = "Artist 1",
                AudioUrl = "url1",
                PublisherId = testUser.Id,
                GenreId = genre.Id
            };

            var song2 = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Song 2",
                Artist = "Artist 2",
                AudioUrl = "url2",
                PublisherId = testUser.Id,
                GenreId = genre.Id
            };

            await dbContext.Songs.AddRangeAsync(song1, song2);

            // Add playlist
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
            await dbContext.Playlists.AddAsync(playlist);

            // Link songs to playlist via PlaylistSong
            playlist.PlaylistsSongs.Add(new PlaylistSong { PlaylistId = playlist.Id, SongId = song1.Id });
            playlist.PlaylistsSongs.Add(new PlaylistSong { PlaylistId = playlist.Id, SongId = song2.Id });

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

        [Test]
        public async Task RemoveSongAsync_RemovesExistingSong()
        {
            var song = new Song
            {
                Id = Guid.NewGuid(),
                Title = "Test Song",
                Artist = "Test Artist",           
                AudioUrl = "/audio/test.mp3",     
                PublisherId = "publisher1",      
                ImageUrl = null                   
            };

            var playlist = await dbContext.Playlists.FirstAsync();
            playlist.PlaylistsSongs.Add(new PlaylistSong { SongId = song.Id, PlaylistId = playlist.Id });
            dbContext.Songs.Add(song);
            await dbContext.SaveChangesAsync();

            var result = await playlistsService.RemoveSongAsync(playlist.Id, song.Id);

            Assert.IsTrue(result);
            Assert.IsFalse(await dbContext.PlaylistsSongs.AnyAsync(ps => ps.SongId == song.Id));
        }

        [Test]
        public async Task EditPlaylistAsync_UpdatesTitleAndImage()
        {
            var playlist = await dbContext.Playlists.FirstAsync();

            var input = new EditPlaylistInputModel
            {
                Id = playlist.Id,
                Title = "Updated Title",
                ImageUrl = "new.jpg"
            };

            var result = await playlistsService.EditPlaylistAsync(input);

            Assert.IsTrue(result);
            var updated = await dbContext.Playlists.FindAsync(playlist.Id);
            Assert.AreEqual("Updated Title", updated.Title);
            Assert.AreEqual("new.jpg", updated.ImageUrl);
        }

        [Test]
        public async Task GetPlaylistDetailsAsync_ReturnsCorrectViewModel_WhenPlaylistExists()
        {
            // Arrange
            var playlist = await dbContext.Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(playlist, "Test playlist should exist in setup");

            // Act
            var result = await playlistsService.GetPlaylistDetailsAsync(playlist.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(playlist.Id, result!.Id);
            Assert.AreEqual(playlist.Title, result.Title);
            Assert.AreEqual(playlist.IsDefault, result.IsDeafault);

            // Songs in playlist
            var expectedSongsCount = playlist.PlaylistsSongs.Count;
            Assert.AreEqual(expectedSongsCount, result.Songs.Count);

            // Verify song properties and default image fallback
            foreach (var songVm in result.Songs)
            {
                var song = playlist.PlaylistsSongs.Select(ps => ps.Song).FirstOrDefault(s => s.Id == songVm.Id);
                Assert.IsNotNull(song);

                Assert.AreEqual(song!.Title, songVm.Title);
                Assert.AreEqual(song.Artist, songVm.Artist);

                // ImageUrl fallback check
                var expectedImageUrl = string.IsNullOrEmpty(song.ImageUrl) ? "/images/no-image.jpg" : song.ImageUrl;
                Assert.AreEqual(expectedImageUrl, songVm.ImageUrl);
            }

            // Available songs should include songs NOT in playlist and not deleted
            var allSongs = await dbContext.Songs.Where(s => !s.IsDeleted).ToListAsync();
            var songsInPlaylistIds = playlist.PlaylistsSongs.Select(ps => ps.SongId).ToHashSet();
            var expectedAvailableSongs = allSongs.Where(s => !songsInPlaylistIds.Contains(s.Id)).ToList();

            Assert.AreEqual(expectedAvailableSongs.Count, result.AvailableSongs.Count);

            foreach (var availableSongVm in result.AvailableSongs)
            {
                var matchingSong = expectedAvailableSongs.FirstOrDefault(s => s.Id == availableSongVm.Id);
                Assert.IsNotNull(matchingSong);

                var expectedImageUrl = string.IsNullOrEmpty(matchingSong.ImageUrl) ? "/images/no-image.jpg" : matchingSong.ImageUrl;
                Assert.AreEqual(expectedImageUrl, availableSongVm.ImageUrl);
            }
        }

        [Test]
        public async Task GetPlaylistDetailsAsync_ReturnsNull_WhenPlaylistDoesNotExist()
        {
            // Arrange
            var nonExistentPlaylistId = Guid.NewGuid();

            // Act
            var result = await playlistsService.GetPlaylistDetailsAsync(nonExistentPlaylistId);

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public async Task AddSongToFavoritesAsync_AddsSong_WhenNotExists()
        {
            // Arrange
            var songId = Guid.NewGuid();

            // Add default favorites playlist
            var favorites = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = "Favorites",
                UserId = testUser.Id,
                IsDefault = true,
                PlaylistsSongs = new List<PlaylistSong>()
            };
            dbContext.Playlists.Add(favorites);
            await dbContext.SaveChangesAsync();

            // Act
            await playlistsService.AddSongToFavoritesAsync(testUser.Id, songId);

            // Assert
            var updatedFavorites = await dbContext.Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstAsync(p => p.UserId == testUser.Id && p.IsDefault);

            Assert.IsTrue(updatedFavorites.PlaylistsSongs.Any(ps => ps.SongId == songId));
        }

        [Test]
        public async Task AddSongToFavoritesAsync_DoesNotAddDuplicateSong()
        {
            var songId = Guid.NewGuid();

            var favorites = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = "Favorites",
                UserId = testUser.Id,
                IsDefault = true,
                PlaylistsSongs = new List<PlaylistSong>()
            {
                new PlaylistSong { SongId = songId }
            }
            };
            dbContext.Playlists.Add(favorites);
            await dbContext.SaveChangesAsync();

            await playlistsService.AddSongToFavoritesAsync(testUser.Id, songId);

            var updatedFavorites = await dbContext.Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstAsync(p => p.UserId == testUser.Id && p.IsDefault);

            int count = updatedFavorites.PlaylistsSongs.Count(ps => ps.SongId == songId);
            Assert.AreEqual(1, count); // No duplicate added
        }

        [Test]
        public async Task RemoveSongFromFavoritesAsync_RemovesSong_WhenExists()
        {
            var songId = Guid.NewGuid();

            var favorites = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = "Favorites",
                UserId = testUser.Id,
                IsDefault = true,
                PlaylistsSongs = new List<PlaylistSong>()
            {
                new PlaylistSong { PlaylistId = Guid.NewGuid(), SongId = songId }
            }
            };
            dbContext.Playlists.Add(favorites);
            await dbContext.SaveChangesAsync();

            await playlistsService.RemoveSongFromFavoritesAsync(testUser.Id, songId);

            var updatedFavorites = await dbContext.Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstAsync(p => p.UserId == testUser.Id && p.IsDefault);

            Assert.IsFalse(updatedFavorites.PlaylistsSongs.Any(ps => ps.SongId == songId));
        }

        [Test]
        public async Task RemoveSongFromFavoritesAsync_DoesNothing_WhenSongNotExists()
        {
            var songId = Guid.NewGuid();

            var favorites = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = "Favorites",
                UserId = testUser.Id,
                IsDefault = true,
                PlaylistsSongs = new List<PlaylistSong>()
            };
            dbContext.Playlists.Add(favorites);
            await dbContext.SaveChangesAsync();

            // No exception should be thrown, and playlist remains unchanged
            await playlistsService.RemoveSongFromFavoritesAsync(testUser.Id, songId);

            var updatedFavorites = await dbContext.Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstAsync(p => p.UserId == testUser.Id && p.IsDefault);

            Assert.IsEmpty(updatedFavorites.PlaylistsSongs);
        }

        [Test]
        public async Task IsSongFavoritesAsync_ReturnsTrue_WhenSongIsInFavorites()
        {
            var songId = Guid.NewGuid();

            var favorites = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = "Favorites",
                UserId = testUser.Id,
                IsDefault = true,
                PlaylistsSongs = new List<PlaylistSong>()
            {
                new PlaylistSong { SongId = songId }
            }
            };
            dbContext.Playlists.Add(favorites);
            await dbContext.SaveChangesAsync();

            bool isFavorite = await playlistsService.IsSongFavoritesAsync(testUser.Id, songId);

            Assert.IsTrue(isFavorite);
        }

        [Test]
        public async Task IsSongFavoritesAsync_ReturnsFalse_WhenSongIsNotInFavorites()
        {
            var songId = Guid.NewGuid();

            var favorites = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = "Favorites",
                UserId = testUser.Id,
                IsDefault = true,
                PlaylistsSongs = new List<PlaylistSong>()
            };
            dbContext.Playlists.Add(favorites);
            await dbContext.SaveChangesAsync();

            bool isFavorite = await playlistsService.IsSongFavoritesAsync(testUser.Id, songId);

            Assert.IsFalse(isFavorite);
        }

        [Test]
        public async Task AddSongsToPlaylistAsync_AddsOnlyNewSongs()
        {
            var playlist = await dbContext.Playlists.FirstAsync();

            var existingSongId = Guid.NewGuid();
            var newSongId = Guid.NewGuid();

            // Setup existing song in playlist
            playlist.PlaylistsSongs.Add(new PlaylistSong { PlaylistId = playlist.Id, SongId = existingSongId });
            await dbContext.SaveChangesAsync();

            var songsToAdd = new List<Guid> { existingSongId, newSongId };

            await playlistsService.AddSongsToPlaylistAsync(playlist.Id, songsToAdd);

            var updatedPlaylist = await dbContext.Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstAsync(p => p.Id == playlist.Id);

            Assert.IsTrue(updatedPlaylist.PlaylistsSongs.Any(ps => ps.SongId == newSongId));
            // existing song should not be duplicated
            int count = updatedPlaylist.PlaylistsSongs.Count(ps => ps.SongId == existingSongId);
            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task GetPlaylistToEditAsync_ReturnsEditModel_WhenPlaylistExists()
        {
            var playlist = await dbContext.Playlists.FirstAsync();

            var result = await playlistsService.GetPlaylistToEditAsync(testUser.Id, playlist.Id.ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(playlist.Title, result.Title);
            Assert.AreEqual(playlist.ImageUrl, result.ImageUrl);
        }

        [Test]
        public async Task GetPlaylistToEditAsync_ReturnsNull_WhenInvalidGuid()
        {
            var result = await playlistsService.GetPlaylistToEditAsync(testUser.Id, "invalid-guid");
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPlaylistToEditAsync_ReturnsNull_WhenPlaylistNotFound()
        {
            var unknownId = Guid.NewGuid().ToString();
            var result = await playlistsService.GetPlaylistToEditAsync(testUser.Id, unknownId);
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllSongsInPlaylistAssync_ReturnsAllSongs()
        {
            var playlist = await dbContext.Playlists.FirstAsync();

            var result = await playlistsService.GetAllSongsInPlaylistAssync(playlist.Id.ToString());

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(s => s.Title == "Song 1"));
            Assert.IsTrue(result.Any(s => s.Title == "Song 2"));
        }

        [Test]
        public async Task GetPlaylistToDeleteAsync_WithValidUserAndPlaylist_ReturnsDeletePlaylistViewModel()
        {
            // Arrange
            var playlist = dbContext.Playlists.First();
            string playlistId = playlist.Id.ToString();
            string userId = testUser.Id;

            // Act
            var result = await playlistsService.GetPlaylistToDeleteAsync(userId, playlistId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(playlist.Id, result.Id);
            Assert.AreEqual(playlist.Title, result.Title);
        }

        [Test]
        public async Task GetPlaylistToDeleteAsync_WithNullPlaylistId_ReturnsNull()
        {
            var result = await playlistsService.GetPlaylistToDeleteAsync(testUser.Id, null);
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetPlaylistToDeleteAsync_WithNonexistentPlaylist_ReturnsNull()
        {
            var fakePlaylistId = Guid.NewGuid().ToString();
            var result = await playlistsService.GetPlaylistToDeleteAsync(testUser.Id, fakePlaylistId);
            Assert.IsNull(result);
        }
    }
}
