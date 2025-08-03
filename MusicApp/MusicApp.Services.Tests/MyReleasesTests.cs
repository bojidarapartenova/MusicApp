using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core;
using MusicApp.Web.ViewModels.Song;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApp.Tests.Services
{
    [TestFixture]
    public class MyReleasesServiceTests
    {
        private MusicAppDbContext dbContext;
        private Mock<UserManager<ApplicationUser>> userManagerMock;
        private MyReleasesService myReleasesService;

        private ApplicationUser publisherUser;
        private ApplicationUser otherUser;
        private Genre genre;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new MusicAppDbContext(options);

            // Mock UserManager (constructor requirement)
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            myReleasesService = new MyReleasesService(dbContext, userManagerMock.Object);

            // Seed data
            publisherUser = new ApplicationUser { Id = "publisher1", UserName = "PublisherUser" };
            otherUser = new ApplicationUser { Id = "user2", UserName = "OtherUser" };

            genre = new Genre { Id = 1, Name = "Pop" };

            dbContext.Users.AddRange(publisherUser, otherUser);
            dbContext.Genres.Add(genre);

            dbContext.Songs.AddRange(new List<Song>
            {
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "First Song",
                    Artist = "Artist 1",
                    AudioUrl = "http://audio1.mp3",
                    ImageUrl = "http://image1.jpg",
                    PublisherId = publisherUser.Id,
                    Publisher = publisherUser,
                    Genre = genre,
                    GenreId = genre.Id,
                    ReleaseDate = DateTime.UtcNow.AddDays(-5)
                },
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "Second Song",
                    Artist = "Artist 2",
                    AudioUrl = "http://audio2.mp3",
                    ImageUrl = "http://image2.jpg",
                    PublisherId = publisherUser.Id,
                    Publisher = publisherUser,
                    Genre = genre,
                    GenreId = genre.Id,
                    ReleaseDate = DateTime.UtcNow.AddDays(-2)
                },
                new Song
                {
                    Id = Guid.NewGuid(),
                    Title = "Other User's Song",
                    Artist = "Other Artist",
                    AudioUrl = "http://audio3.mp3",
                    ImageUrl = "http://image3.jpg",
                    PublisherId = otherUser.Id,
                    Publisher = otherUser,
                    Genre = genre,
                    GenreId = genre.Id,
                    ReleaseDate = DateTime.UtcNow.AddDays(-1)
                }
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
        public async Task GetAllMyReleasesAsync_ReturnsOnlyUserSongs_OrderedByReleaseDate()
        {
            // Act
            var result = (await myReleasesService.GetAllMyReleasesAsync(publisherUser.Id)).ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Second Song", result[0].Title); // newest first
            Assert.AreEqual("First Song", result[1].Title);  // older second
        }

        [Test]
        public async Task GetAllMyReleasesAsync_ReturnsEmpty_WhenUserHasNoSongs()
        {
            // Act
            var result = await myReleasesService.GetAllMyReleasesAsync("nonexistentUser");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAllMyReleasesAsync_ReturnsCorrectPublisherInfo()
        {
            // Act
            var result = await myReleasesService.GetAllMyReleasesAsync(publisherUser.Id);
            var song = result.FirstOrDefault();

            // Assert
            Assert.IsNotNull(song);
            Assert.AreEqual(publisherUser.Id, song.PublisherId);
            Assert.AreEqual(publisherUser.UserName, song.Publisher);
        }

        [Test]
        public async Task GetAllMyReleasesAsync_ReturnsCorrectGenreName()
        {
            // Act
            var result = await myReleasesService.GetAllMyReleasesAsync(publisherUser.Id);

            // Assert
            Assert.IsTrue(result.All(s => s.Genre == "Pop"));
        }
    }
}
