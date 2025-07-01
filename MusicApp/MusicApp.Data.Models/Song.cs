namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Song
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = null!;
        public int Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Likes { get; set; }
        public string? ImageUrl { get; set; }
        [Required]
        public string AudioUrl { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;

        public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public ICollection<PlaylistSong> PlaylistsSongs { get; set; } = new HashSet<PlaylistSong>();
    }
}
