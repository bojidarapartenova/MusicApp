namespace MusicApp.Data.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using MusicApp.Data.Models;

    public class MusicAppDbContext : IdentityDbContext<ApplicationUser>
    {
        public MusicAppDbContext(DbContextOptions<MusicAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Playlist> Playlists { get; set; } = null!;
        public DbSet<PlaylistSong> PlaylistsSongs { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PlaylistSong>()
                .HasKey(ps => new { ps.PlaylistId, ps.SongId });

            builder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Playlist)
                .WithMany(ps => ps.PlaylistsSongs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PlaylistSong>()
                .HasOne(ps => ps.Song)
                .WithMany(ps => ps.PlaylistsSongs)
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PlaylistSong>().HasQueryFilter(ps => ps.Song.IsDeleted == false);

            builder.Entity<Song>()
                .HasQueryFilter(s => s.IsDeleted == false);

            builder.Entity<Comment>()
                .HasQueryFilter(c=>c.Song.IsDeleted==false);

            builder.Entity<Like>()
                .HasQueryFilter(l=>l.Song.IsDeleted==false);

            builder.Entity<Notification>()
                .HasQueryFilter(n => n.Song.IsDeleted == false);

            builder.Entity<Song>()
                .HasData(
                new Song
                {
                    Id = Guid.Parse("e1a3c7d2-b4d9-4f51-8c52-16fa3b2b1c5a"),
                    Title = "Benson Boone - Beautiful Things (Sped Up)",
                    Duration = 154,
                    AudioUrl = "/audio/beautiful_things.mp3",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/en/4/4b/Benson_Boone_-_Beautiful_Things.png",
                    ReleaseDate = new DateTime(2024, 1, 19),
                    Likes = 0
                },
                new Song
                {
                    Id = Guid.Parse("f4e2a8c9-7f38-4cbe-bad4-3c4e5f27893d"),
                    Title = "The Weeknd - Popular (Sped up)",
                    Duration = 187,
                    AudioUrl = "/audio/popular.mp3",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/en/2/24/The_Weeknd_-_Popular.png",
                    ReleaseDate = new DateTime(2023, 6, 2),
                    Likes = 0
                },
                new Song
                {
                    Id = Guid.Parse("a7b1e9c3-3fc2-4d6b-b14a-91d208bc72e1"),
                    Title = "Espresso - Sabrina Carpenter (Sped up)",
                    Duration = 162,
                    AudioUrl = "/audio/espresso.mp3",
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/en/7/71/Espresso_-_Sabrina_Carpenter.png",
                    ReleaseDate = new DateTime(2024, 5, 1),
                    Likes = 0
                });
        }
    }
}
