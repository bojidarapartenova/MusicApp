using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.ViewModels.Playlists
{
    public class CreatePlaylistViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = null!;
        public string? ImageUrl {  get; set; }
        public List<Guid> SelectedSongsIds { get; set; } = new List<Guid>();
        public IEnumerable<SongViewModel> Songs { get; set; }= new List<SongViewModel>();
    }
}
