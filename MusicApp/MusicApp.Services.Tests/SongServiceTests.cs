using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApp.Tests
{
    [TestFixture]
    public class SongServiceTests
    {
        private MusicAppDbContext dbContext;
        private SongService songService;
        private Mock<UserManager<ApplicationUser>> userManagerMock;
        private Mock<IWebHostEnvironment> webHostEnvironmentMock;
        private Mock<INotificationsService> notificationServiceMock;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            dbContext = new MusicAppDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            notificationServiceMock = new Mock<INotificationsService>();

            songService = new SongService(
                dbContext,
                userManagerMock.Object,
                webHostEnvironmentMock.Object,
                notificationServiceMock.Object);
        }

        [Test]
        public async Task GetAllSongsAsync_ReturnsAllSongsSortedByReleaseDate()
        {
            var genre = new Genre { Id = 1, Name = "Pop" };
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };

            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);

            dbContext.Songs.AddRange(new List<Song>
            {
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "First Song",
                    GenreId = genre.Id,
                    Genre = genre,
                    Artist = "Artist A",
                    PublisherId = user.Id,
                    Publisher = user,
                    ReleaseDate = DateTime.UtcNow.AddDays(-1),
                    Likes = 2,
                    AudioUrl = "/audio/test1.mp3",
                    ImageUrl = "/images/img1.jpg"
                },
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "Latest Song",
                    GenreId = genre.Id,
                    Genre = genre,
                    Artist = "Artist B",
                    PublisherId = user.Id,
                    Publisher = user,
                    ReleaseDate = DateTime.UtcNow,
                    Likes = 5,
                    AudioUrl = "/audio/test2.mp3",
                    ImageUrl = "/images/img2.jpg"
                }
            });

            await dbContext.SaveChangesAsync();

            var (songs, totalCount) = await songService.GetAllSongsAsync();

            Assert.That(totalCount, Is.EqualTo(2));
            Assert.That(songs.First().Title, Is.EqualTo("Latest Song"));
        }

        [Test]
        public async Task AddSongAsync_WithValidInput_AddsSongAndReturnsTrue()
        {
            // Arrange
            var userId = "user1";
            var genre = new Genre { Id = 1, Name = "Pop" };
            var user = new ApplicationUser { Id = userId, UserName = "TestUser" };

            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Mock webRootPath
            string webRootPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "wwwroot");
            Directory.CreateDirectory(Path.Combine(webRootPath, "audio"));
            webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(webRootPath);

            // Load the valid MP3 file from testdata folder
            string mp3FilePath = Path.Combine("..", "..", "..", "..", "MusicApp.Web", "wwwroot", "audio", "asitwas.mp3");
            using var mp3Stream = File.OpenRead(mp3FilePath);

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.FileName).Returns("asitwas.mp3");
            formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns<Stream, System.Threading.CancellationToken>((stream, ct) => mp3Stream.CopyToAsync(stream, ct));
            formFileMock.Setup(f => f.Length).Returns(mp3Stream.Length);
            formFileMock.Setup(f => f.OpenReadStream()).Returns(mp3Stream);

            var inputModel = new AddSongInputModel
            {
                Title = "Test Song",
                Artist = "Test Artist",
                GenreId = genre.Id,
                AudioUrl = formFileMock.Object,
                ImageUrl = null
            };

            // Act
            var result = await songService.AddSongAsync(userId, inputModel);

            // Assert
            Assert.IsTrue(result);

            var songInDb = await dbContext.Songs.FirstOrDefaultAsync(s => s.Title == "Test Song");
            Assert.IsNotNull(songInDb);
            Assert.AreEqual("Test Artist", songInDb.Artist);
            Assert.AreEqual(userId, songInDb.PublisherId);
            Assert.AreEqual(genre.Id, songInDb.GenreId);
            Assert.IsNotNull(songInDb.AudioUrl);
            Assert.IsNotEmpty(songInDb.AudioUrl);
        }

        [Test]
        public async Task GetSongToEditAsync_WithValidUserAndSongId_ReturnsEditSongInputModel()
        {
            // Arrange
            var genre = new Genre { Id = 1, Name = "Pop" };
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };
            var songId = Guid.NewGuid();

            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);

            var song = new Song
            {
                Id = songId,
                Title = "Song Title",
                GenreId = genre.Id,
                Genre = genre,
                Artist = "Artist",
                PublisherId = user.Id,
                Publisher = user,
                AudioUrl = "/audio/song.mp3",
                ImageUrl = "/images/image.jpg"
            };

            dbContext.Songs.Add(song);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(um => um.FindByIdAsync(user.Id))
                           .ReturnsAsync(user);

            // Act
            var result = await songService.GetSongToEditAsync(user.Id, songId.ToString());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(song.Id, result.Id);
            Assert.AreEqual(song.Title, result.Title);
            Assert.AreEqual(song.GenreId, result.GenreId);
            Assert.AreEqual(song.Artist, result.Artist);
        }
        [Test]
        public async Task EditSongAsync_WithValidInputAndNewAudioFile_UpdatesSongAndReturnsTrue()
        {
            // Arrange
            var genre = new Genre { Id = 1, Name = "Pop" };
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };
            var songId = Guid.NewGuid();

            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);

            var existingSong = new Song
            {
                Id = songId,
                Title = "Old Title",
                GenreId = genre.Id,
                Genre = genre,
                Artist = "Old Artist",
                PublisherId = user.Id,
                AudioUrl = "/audio/asitwas.mp3",
                ImageUrl = "/images/old.jpg",
                Duration = 200
            };

            dbContext.Songs.Add(existingSong);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(um => um.FindByIdAsync(user.Id)).ReturnsAsync(user);

            // Set up a real-looking old audio file to simulate deletion
            string webRootPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "wwwroot");
            string audioDir = Path.Combine(webRootPath, "audio");
            Directory.CreateDirectory(audioDir);
            string oldAudioPath = Path.Combine(audioDir, "asitwas.mp3");
            await File.WriteAllBytesAsync(oldAudioPath, new byte[] { 0x0 }); // dummy byte file
            webHostEnvironmentMock.Setup(w => w.WebRootPath).Returns(webRootPath);

            // Prepare new audio file (real MP3)
            string mp3FilePath = Path.Combine("..", "..", "..", "..", "MusicApp.Web", "wwwroot", "audio", "levitating.mp3");
            Assert.That(File.Exists(mp3FilePath), $"MP3 test file not found at path: {mp3FilePath}");

            await using var mp3Stream = File.OpenRead(mp3FilePath);
            var memoryStream = new MemoryStream();
            await mp3Stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.FileName).Returns("levitating.mp3");
            formFileMock.Setup(f => f.Length).Returns(memoryStream.Length);
            formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                        .Returns<Stream, System.Threading.CancellationToken>((stream, ct) =>
                        {
                            memoryStream.Position = 0;
                            return memoryStream.CopyToAsync(stream, ct);
                        });
            formFileMock.Setup(f => f.OpenReadStream()).Returns(() =>
            {
                memoryStream.Position = 0;
                return memoryStream;
            });

            var inputModel = new EditSongInputModel
            {
                Id = songId,
                Title = "New Title",
                GenreId = genre.Id,
                Artist = "New Artist",
                ImageUrl = "/images/new.jpg",
                PublisherId = user.Id,
                NewAudioFile = formFileMock.Object
            };

            // Act
            var result = await songService.EditSongAsync(inputModel);

            // Assert
            Assert.IsTrue(result);
            var updatedSong = await dbContext.Songs.FindAsync(songId);
            Assert.AreEqual("New Title", updatedSong.Title);
            Assert.AreEqual("New Artist", updatedSong.Artist);
            Assert.AreEqual("/images/new.jpg", updatedSong.ImageUrl);
            Assert.That(updatedSong.AudioUrl, Does.Contain("/audio/"));
            Assert.That(updatedSong.Duration, Is.GreaterThan(0));
        }


        [Test]
        public async Task EditSongAsync_WithoutNewAudioFile_UpdatesSongAndReturnsTrue()
        {
            // Arrange same as above but NewAudioFile = null
            var genre = new Genre { Id = 1, Name = "Pop" };
            var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };
            var songId = Guid.NewGuid();

            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);

            var existingSong = new Song
            {
                Id = songId,
                Title = "Old Title",
                GenreId = genre.Id,
                Genre = genre,
                Artist = "Old Artist",
                PublisherId = user.Id,
                AudioUrl = "/audio/oldfile.mp3",
                ImageUrl = "/images/old.jpg",
                Duration = 200
            };

            dbContext.Songs.Add(existingSong);
            await dbContext.SaveChangesAsync();

            var inputModel = new EditSongInputModel
            {
                Id = songId,
                Title = "Updated Title",
                GenreId = genre.Id,
                Artist = "Updated Artist",
                ImageUrl = "/images/updated.jpg",
                PublisherId = user.Id,
                NewAudioFile = null
            };

            // Act
            var result = await songService.EditSongAsync(inputModel);

            // Assert
            Assert.IsTrue(result);
            var updatedSong = await dbContext.Songs.FindAsync(songId);
            Assert.AreEqual("Updated Title", updatedSong.Title);
            Assert.AreEqual("Updated Artist", updatedSong.Artist);
            Assert.AreEqual("/images/updated.jpg", updatedSong.ImageUrl);
            Assert.AreEqual("/audio/oldfile.mp3", updatedSong.AudioUrl); // AudioUrl remains same
            Assert.AreEqual(200, updatedSong.Duration); // Duration remains same
        }

        [Test]
        public async Task GetSongByIdAsync_WithValidId_ReturnsSongViewModel()
        {
            var genre = new Genre { Id = 1, Name = "Rock" };
            var user = new ApplicationUser { Id = "user1", UserName = "Publisher" };
            var songId = Guid.NewGuid();

            var song = new Song
            {
                Id = songId,
                Title = "Rock Anthem",
                GenreId = genre.Id,
                Genre = genre,
                Artist = "Band",
                PublisherId = user.Id,
                Publisher = user,
                AudioUrl = "/audio/rock.mp3",
                ImageUrl = "/images/rock.jpg",
                Duration = 300,
                ReleaseDate = DateTime.UtcNow,
                Likes = 10
            };

            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);
            dbContext.Songs.Add(song);
            await dbContext.SaveChangesAsync();

            var result = await songService.GetSongByIdAsync(songId.ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(songId, result.Id);
            Assert.AreEqual("Rock Anthem", result.Title);
        }

        [Test]
        public async Task GetSongsCountAsync_ReturnsCorrectCount()
        {
            var genre = new Genre { Id = 1, Name = "Genre" };
            var user = new ApplicationUser { Id = "user1", UserName = "Publisher" };
            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);

            dbContext.Songs.AddRange(
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "Song A",
                    Artist = "Artist A",
                    AudioUrl = "/audio/a.mp3",
                    PublisherId = user.Id,
                    Publisher = user,
                    GenreId = genre.Id,
                    Genre = genre,
                    ReleaseDate = DateTime.UtcNow
                },
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "Song B",
                    Artist = "Artist B",
                    AudioUrl = "/audio/b.mp3",
                    PublisherId = user.Id,
                    Publisher = user,
                    GenreId = genre.Id,
                    Genre = genre,
                    ReleaseDate = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();

            var result = await songService.GetSongsCountAsync();

            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task GetTotalSongsLikesAsync_ReturnsCorrectSum()
        {
            var genre = new Genre { Id = 1, Name = "Genre" };
            var user = new ApplicationUser { Id = "user1", UserName = "Publisher" };
            dbContext.Genres.Add(genre);
            dbContext.Users.Add(user);

            dbContext.Songs.AddRange(
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "Song A",
                    Artist = "Artist A",
                    AudioUrl = "/audio/a.mp3",
                    PublisherId = user.Id,
                    Publisher = user,
                    GenreId = genre.Id,
                    Genre = genre,
                    ReleaseDate = DateTime.UtcNow,
                    Likes = 5
                },
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "Song B",
                    Artist = "Artist B",
                    AudioUrl = "/audio/b.mp3",
                    PublisherId = user.Id,
                    Publisher = user,
                    GenreId = genre.Id,
                    Genre = genre,
                    ReleaseDate = DateTime.UtcNow,
                    Likes = 3
                });

            await dbContext.SaveChangesAsync();

            var result = await songService.GetTotalSongsLikesAsync();

            Assert.AreEqual(8, result);
        }

        [Test]
        public async Task GetLatestSongsAsync_ReturnsCorrectCountAndOrder()
        {
            var user = new ApplicationUser { Id = "user1", UserName = "Publisher" };
            var genre = new Genre { Id = 1, Name = "Jazz" };
            dbContext.Users.Add(user);
            dbContext.Genres.Add(genre);

            for (int i = 0; i < 5; i++)
            {
                dbContext.Songs.Add(new Song
                {
                    Id = Guid.NewGuid(),
                    Title = $"Song {i}",
                    Artist = "Artist",
                    Publisher = user,
                    PublisherId = user.Id,
                    Genre = genre,
                    GenreId = genre.Id,
                    ReleaseDate = DateTime.UtcNow.AddMinutes(-i),
                    ImageUrl = $"/images/{i}.jpg",
                    AudioUrl = $"/audio/{i}.mp3"
                });
            }

            await dbContext.SaveChangesAsync();

            var result = await songService.GetLatestSongsAsync(3);

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("Song 0", result.First().Title);
        }

        [Test]
        public async Task GetSongLikeCountAsync_ReturnsCorrectNumber()
        {
            var user = new ApplicationUser { Id = "user1", UserName = "User" };
            var songId = Guid.NewGuid();
            var genre = new Genre { Id = 1, Name = "Pop" };

            var song = new Song
            {
                Id = songId,
                Title = "Liked Song",
                Artist = "Some Artist",
                PublisherId = user.Id,
                Publisher = user,
                GenreId = genre.Id,
                Genre = genre,
                AudioUrl = "/audio/song.mp3",
                ReleaseDate = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            dbContext.Genres.Add(genre);
            dbContext.Songs.Add(song);

            dbContext.Likes.AddRange(
                new Like { Id = Guid.NewGuid(), SongId = songId, UserId = user.Id, User = user },
                new Like { Id = Guid.NewGuid(), SongId = songId, UserId = user.Id, User = user });

            await dbContext.SaveChangesAsync();

            var result = await songService.GetSongLikeCountAsync(songId);

            Assert.AreEqual(2, result);
        }

        [Test]
        public async Task ToggleLikeAsync_WhenSongIsNotLiked_AddsLike()
        {
            var user = new ApplicationUser { Id = "user1", UserName = "User" };
            var songId = Guid.NewGuid();

            dbContext.Users.Add(user);
            dbContext.Songs.Add(new Song
            {
                Id = songId,
                Title = "Song",
                Artist = "Artist",
                AudioUrl = "/audio/song.mp3",
                PublisherId = user.Id,
                GenreId = 1,
                ReleaseDate = DateTime.UtcNow,
            });
            await dbContext.SaveChangesAsync();

            var result = await songService.ToggleLikeAsync(user.Id, songId);

            Assert.IsTrue(result);
            Assert.AreEqual(1, dbContext.Likes.Count(l => l.SongId == songId && l.UserId == user.Id));
        }

        [Test]
        public async Task ToggleLikeAsync_WhenSongIsAlreadyLiked_RemovesLike()
        {
            var user = new ApplicationUser { Id = "user1", UserName = "User" };
            var songId = Guid.NewGuid();

            dbContext.Users.Add(user);
            dbContext.Likes.Add(new Like { Id = Guid.NewGuid(), SongId = songId, UserId = user.Id });
            await dbContext.SaveChangesAsync();

            var result = await songService.ToggleLikeAsync(user.Id, songId);

            Assert.IsFalse(result);
            Assert.IsFalse(dbContext.Likes.Any(l => l.SongId == songId && l.UserId == user.Id));
        }

      
        [Test]
        public async Task GetSongToDeleteAsync_WithInvalidSongId_ReturnsNull()
        {
            // Arrange
            var userId = "user1";
            var invalidSongId = "not-a-guid";

            var user = new ApplicationUser { Id = userId, UserName = "User" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await songService.GetSongToDeleteAsync(userId, invalidSongId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task SoftDeleteSongAsync_WhenUserIsPublisher_DeletesSongAndReturnsTrue()
        {
            // Arrange
            var userId = "user1";
            var songId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "user1" };
            var song = new Song
            {
                Id = songId,
                PublisherId = userId,
                IsDeleted = false,
                Artist = "Test Artist",
                AudioUrl = "http://test-audio-url",
                Title = "Test Title"
            };


            dbContext.Songs.Add(song);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var viewModel = new DeleteSongViewModel { Id = songId };

            // Act
            var result = await songService.SoftDeleteSongAsync(userId, viewModel);

            // Assert
            Assert.IsTrue(result);
            var deletedSong = await dbContext.Songs.FindAsync(songId);
            Assert.IsTrue(deletedSong.IsDeleted);
        }

        [Test]
        public async Task SoftDeleteSongAsync_WhenUserIsAdmin_DeletesSongAndReturnsTrue()
        {
            // Arrange
            var userId = "adminUser";
            var songId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, UserName = "adminUser" };
            var song = new Song
            {
                Id = songId,
                PublisherId = userId,
                IsDeleted = false,
                Artist = "Test Artist",
                AudioUrl = "http://test-audio-url",
                Title = "Test Title"
            };

            dbContext.Songs.Add(song);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

            var viewModel = new DeleteSongViewModel { Id = songId };

            // Act
            var result = await songService.SoftDeleteSongAsync(userId, viewModel);

            // Assert
            Assert.IsTrue(result);
            var deletedSong = await dbContext.Songs.FindAsync(songId);
            Assert.IsTrue(deletedSong.IsDeleted);
        }

        [Test]
        public async Task SoftDeleteSongAsync_WhenUserIsNull_ReturnsFalse()
        {
            // Arrange
            var userId = "userDoesNotExist";
            var viewModel = new DeleteSongViewModel { Id = Guid.NewGuid() };

            userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await songService.SoftDeleteSongAsync(userId, viewModel);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task SoftDeleteSongAsync_WhenSongIsNull_ReturnsFalse()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser { Id = userId };

            userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);

            var viewModel = new DeleteSongViewModel { Id = Guid.NewGuid() }; // No song with this Id in DB

            // Act
            var result = await songService.SoftDeleteSongAsync(userId, viewModel);

            // Assert
            Assert.IsFalse(result);
        }

    }
}
