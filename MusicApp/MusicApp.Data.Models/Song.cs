namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Song
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = null!;

        [Required]
        public string PublisherId { get; set; } = null!;
        [Required]
        [ForeignKey(nameof(PublisherId))]
        public ApplicationUser Publisher { get; set; } = null!;

        [Required]
        public int GenreId {  get; set; }
        [Required]
        [MaxLength(20)]
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Artist {  get; set; } = null!;

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
