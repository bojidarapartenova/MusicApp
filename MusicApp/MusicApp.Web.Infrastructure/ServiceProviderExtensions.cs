namespace MusicApp.Web.Infrastructure
{
    using Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using MusicApp.Data.Data;
    using static Common.ApplicationConstants;

    public static class ServiceProviderExtensions
    {
        public static async Task SeedAdminAsync(this IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<MusicAppDbContext>();

            bool isAdminRoleExisting = await roleManager
                .RoleExistsAsync(AdminRoleName);
            if (!isAdminRoleExisting)
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
            }

            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser()
                {
                    UserName = AdminUsername,
                    Email = AdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, AdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, AdminRoleName);
                    adminUser = newAdmin;
                }
            }
            var songs = GetSeedSongs(adminUser!.Id);

            foreach(var song in songs)
            {
                bool exists= await dbContext
                    .Songs
                    .IgnoreQueryFilters()
                    .AnyAsync(s=>s.Title == song.Title);

                if (!exists)
                {
                    dbContext.Songs.Add(song);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedUsersAsync(this IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<MusicAppDbContext>();

            const string userEmail = "user@example.com";
            const string userPassword = "123456";
            const string username = "UserExample";

            var existingUser = await userManager.FindByEmailAsync(userEmail);

            ApplicationUser seedUser;
            if (existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = username,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, userPassword);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create seed user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                seedUser = newUser;
            }
            else
            {
                seedUser = existingUser;
            }

            var targetSong = await dbContext.Songs
                .FirstOrDefaultAsync(s => s.Title == "Blinding Lights (Slowed)");

            if (targetSong != null)
            {
                bool alreadyCommented = await dbContext.Comments
                    .AnyAsync(c => c.SongId == targetSong.Id && c.UserId == seedUser.Id);

                if (!alreadyCommented)
                {
                    var comment = new Comment
                    {
                        Id = Guid.NewGuid(),
                        SongId = targetSong.Id,
                        Text = "This version gives the song a whole new vibe — love it!",
                        UserId = seedUser.Id,
                        CreatedOn = DateTime.UtcNow
                    };

                    dbContext.Comments.Add(comment);
                    await dbContext.SaveChangesAsync();
                }
            }
        }



        private static List<Song> GetSeedSongs(string publisherId)
        {
            return new List<Song>
    {
        new Song
        {
            Id = Guid.NewGuid(),
            Title = "Blinding Lights (Slowed)",
            PublisherId = publisherId,
            GenreId = 2,
            Artist = "The Weeknd",
            Duration = 231,
            ReleaseDate = new DateTime(2019, 11, 29),
            Likes = 0,
            AudioUrl = "/audio/blindinglights.mp3",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/en/e/e6/The_Weeknd_-_Blinding_Lights.png"
        },
        new Song
        {
            Id = Guid.NewGuid(),
            Title = "As It Was (Slowed)",
            PublisherId = publisherId,
            GenreId = 1,
            Artist = "Harry Styles",
            Duration = 203,
            ReleaseDate = new DateTime(2022, 4, 1),
            Likes = 0,
            AudioUrl = "/audio/asitwas.mp3",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/en/f/ff/Harry_Styles_-_As_It_Was.png"
        },
        new Song
        {
            Id = Guid.NewGuid(),
            Title = "Levitating (Sped Up)",
            PublisherId = publisherId,
            GenreId = 1,
            Artist = "Dua Lipa",
            Duration = 185,
            ReleaseDate = new DateTime(2020, 10, 1),
            Likes = 0,
            AudioUrl = "/audio/levitating.mp3",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/en/3/3d/Dua_Lipa_Levitating_%28DaBaby_Remix%29.png"
        },
        new Song
        {
            Id = Guid.NewGuid(),
            Title = "Heat Waves (Sped Up)",
            PublisherId = publisherId,
            GenreId = 5,
            Artist = "Glass Animals",
            Duration = 176,
            ReleaseDate = new DateTime(2020, 6, 29),
            Likes = 0,
            AudioUrl = "/audio/heatwaves.mp3",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/en/b/b0/Glass_Animals_-_Heat_Waves.png"
        },
        new Song
        {
            Id = Guid.NewGuid(),
            Title = "Smells Like Teen Spirit (Slowed)",
            PublisherId = publisherId,
            GenreId = 4,
            Artist = "Nirvana",
            Duration = 329,
            ReleaseDate = new DateTime(1991, 9, 10),
            Likes = 0,
            AudioUrl = "/audio/smellsliketeenspirit.mp3",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/en/3/3c/Smells_Like_Teen_Spirit.jpg"
        }
    };
        }

    }
}
