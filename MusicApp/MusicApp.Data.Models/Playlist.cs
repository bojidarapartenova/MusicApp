namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Playlist
    {
        [Key]
        public Guid Id { get; set; }
        public string? CoverUrl { get; set; }

        [MaxLength(50)]
        public string Title { get; set; } = null!;
        public int Likes { get; set; }

        public HashSet<PlaylistSong> PlaylistsSongs { get; set; } = new HashSet<PlaylistSong>();
    }
}
