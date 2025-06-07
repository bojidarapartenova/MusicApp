namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Song
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string Title { get; set; } = null!;
        public int Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Likes { get; set; }
        public string? CoverUrl { get; set; }

        public HashSet<Comment> Comments { get; set; } = new HashSet<Comment>();

        public HashSet<PlaylistSong> PlaylistsSongs { get; set; } = new HashSet<PlaylistSong>();
    }
}
