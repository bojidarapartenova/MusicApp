namespace MusicApp.Data.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using MusicApp.Data.Models;

    public class MusicAppDbContext : IdentityDbContext<IdentityUser>
    {
        public MusicAppDbContext(DbContextOptions<MusicAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Playlist> Playlists { get; set; } = null!;
        public DbSet<PlaylistSong> PlaylistsSongs { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PlaylistSong>()
                .HasKey(pk => new { pk.PlaylistId, pk.SongId });
        }
    }
}
