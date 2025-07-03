namespace MusicApp.Data.Data
{
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
        public DbSet<Genre> Genres { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Song>()
                .HasOne(s => s.Publisher)
                .WithMany(p => p.UploadedSongs)
                .HasForeignKey(s => s.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

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
                .HasQueryFilter(c => c.Song.IsDeleted == false);

            builder.Entity<Like>()
                .HasQueryFilter(l => l.Song.IsDeleted == false);

            builder.Entity<Notification>()
                .HasQueryFilter(n => n.Song.IsDeleted == false);

            builder.Entity<Song>()
                .HasOne(s => s.Genre)
                .WithMany(g => g.Songs)
                .HasForeignKey(s => s.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Genre>()
                .HasData(
                new Genre { Id = 1, Name = "Pop" },
                new Genre { Id = 2, Name = "R&B" },
                new Genre { Id = 3, Name = "Hip Hop" },
                new Genre { Id = 4, Name = "Rock" },
                new Genre { Id = 5, Name = "Electronic" }
               );
        }
    }
}
