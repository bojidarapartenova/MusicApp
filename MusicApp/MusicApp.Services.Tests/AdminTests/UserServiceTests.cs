using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Admin;
using MusicApp.Web.ViewModels.Admin.UserManagement;
using NUnit.Framework;

namespace MusicApp.Services.Tests.Services.Admin
{
    [TestFixture]
    public class UserServiceTests
    {
        private MusicAppDbContext dbContext = null!;
        private Mock<UserManager<ApplicationUser>> userManagerMock = null!;
        private UserService userService = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MusicAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new MusicAppDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            userService = new UserService(dbContext, userManagerMock.Object);
        }

        [Test]
        public async Task GetUsersCountAsync_ReturnsCorrectCount()
        {
            dbContext.Users.Add(new ApplicationUser { Id = "1", UserName = "User1", Email = "user1@test.com" });
            dbContext.Users.Add(new ApplicationUser { Id = "2", UserName = "User2", Email = "user2@test.com" });
            await dbContext.SaveChangesAsync();

            var count = await userService.GetUsersCountAsync();

            Assert.AreEqual(2, count);
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsAllUsersWithoutFilters()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", UserName = "User1", Email = "user1@test.com", LockoutEnabled = false },
                new ApplicationUser { Id = "2", UserName = "User2", Email = "user2@test.com", LockoutEnabled = true, LockoutEnd = DateTimeOffset.MaxValue }
            };
            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>()); // no roles

            userManagerMock.Setup(um => um.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
                .ReturnsAsync(false);

            var result = await userService.GetAllUsersAsync(null, null);

            Assert.AreEqual(2, result.Count());
            Assert.IsFalse(result.First().IsAdmin);
            Assert.IsTrue(result.First().IsActive);
            Assert.IsFalse(result.Last().IsActive); // Locked out
        }

        [Test]
        public async Task GetAllUsersAsync_WithRoleFilterAdmin_ReturnsOnlyAdmins()
        {
            var user1 = new ApplicationUser { Id = "1", UserName = "User1", Email = "user1@test.com" };
            var user2 = new ApplicationUser { Id = "2", UserName = "User2", Email = "user2@test.com" };
            await dbContext.Users.AddRangeAsync(user1, user2);
            await dbContext.SaveChangesAsync();

            userManagerMock.Setup(um => um.GetRolesAsync(user1)).ReturnsAsync(new List<string> { "Admin" });
            userManagerMock.Setup(um => um.GetRolesAsync(user2)).ReturnsAsync(new List<string>());

            userManagerMock.Setup(um => um.IsInRoleAsync(user1, "Admin")).ReturnsAsync(true);
            userManagerMock.Setup(um => um.IsInRoleAsync(user2, "Admin")).ReturnsAsync(false);

            var result = await userService.GetAllUsersAsync(null, "Admin");

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("1", result.First().Id);
            Assert.IsTrue(result.First().IsAdmin);
        }

        [Test]
        public async Task MakeAdminAsync_AddsAdminRoleIfNotInRole()
        {
            var user = new ApplicationUser { Id = "1", UserName = "User1" };
            userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            userManagerMock.Setup(um => um.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);

            await userService.MakeAdminAsync("1");

            userManagerMock.Verify(um => um.AddToRoleAsync(user, "Admin"), Times.Once);
        }

        [Test]
        public async Task RemoveAdminAsync_RemovesAdminRoleIfInRole()
        {
            var user = new ApplicationUser { Id = "1", UserName = "User1" };
            userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            userManagerMock.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
            userManagerMock.Setup(um => um.RemoveFromRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);

            await userService.RemoveAdminAsync("1");

            userManagerMock.Verify(um => um.RemoveFromRoleAsync(user, "Admin"), Times.Once);
        }

        [Test]
        public async Task ToggleActivationAsync_LocksAndUnlocksUserCorrectly()
        {
            var user = new ApplicationUser { Id = "1", UserName = "User1", LockoutEnabled = true, LockoutEnd = DateTimeOffset.MaxValue };
            userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            userManagerMock.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Unlock user
            await userService.ToggleActivationAsync("1");
            Assert.IsFalse(user.LockoutEnabled);

            // Lock user
            await userService.ToggleActivationAsync("1");
            Assert.IsTrue(user.LockoutEnabled);
            Assert.AreEqual(DateTimeOffset.MaxValue, user.LockoutEnd);

            userManagerMock.Verify(um => um.UpdateAsync(user), Times.Exactly(2));
        }
    }
}
