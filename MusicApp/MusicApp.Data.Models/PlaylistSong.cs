namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PlaylistSong
    {
        public Guid PlaylistId { get; set; }
        [Required]
        [ForeignKey(nameof(PlaylistId))]
        public Playlist Playlist { get; set; } = null!;

        public Guid SongId { get; set; }
        [Required]
        [ForeignKey(nameof(SongId))]
        public Song Song { get; set; } = null!;
    }
}
