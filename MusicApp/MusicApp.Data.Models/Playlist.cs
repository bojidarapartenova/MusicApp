namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Playlist
    {
        [Key]
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = null!;

        public bool IsFavorites {  get; set; }

        public ICollection<PlaylistSong> PlaylistsSongs { get; set; } = new HashSet<PlaylistSong>();
    }
}
