namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Playlist
    {
        [Key]
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public ICollection<PlaylistSong> PlaylistsSongs { get; set; } = new HashSet<PlaylistSong>();
    }
}
