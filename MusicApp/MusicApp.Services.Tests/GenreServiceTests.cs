using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core;
using MusicApp.Web.ViewModels.Song;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApp.Tests.Services
{
    [TestFixture]
    public class GenreServiceTests
    {
        private MusicAppDbContext dbContext;
        private GenreService genreService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestMusicAppDb")
                .Options;

            dbContext = new MusicAppDbContext(options);

            // Seed the in-memory database
            dbContext.Genres.AddRange(new List<Genre>
            {
                new Genre { Id = 1, Name = "Rock" },
                new Genre { Id = 2, Name = "Pop" },
                new Genre { Id = 3, Name = "Jazz" },
            });
            dbContext.SaveChanges();

            genreService = new GenreService(dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }

        [Test]
        public async Task GetGenresDropDownAsync_ReturnsAllGenres()
        {
            // Act
            var result = await genreService.GetGenresDropDownAsync();

            // Assert
            Assert.IsNotNull(result);
            var genresList = result.ToList();
            Assert.AreEqual(3, genresList.Count);
            Assert.IsTrue(genresList.Any(g => g.GenreId == 1 && g.Name == "Rock"));
            Assert.IsTrue(genresList.Any(g => g.GenreId == 2 && g.Name == "Pop"));
            Assert.IsTrue(genresList.Any(g => g.GenreId == 3 && g.Name == "Jazz"));
        }

        [Test]
        public async Task GetGenresDropDownAsync_ReturnsEmpty_WhenNoGenres()
        {
            // Arrange - clear genres
            dbContext.Genres.RemoveRange(dbContext.Genres);
            dbContext.SaveChanges();

            // Act
            var result = await genreService.GetGenresDropDownAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }
    }
}
