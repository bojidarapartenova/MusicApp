using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Playlists
{
    public class NewPlaylistViewModel
    {
        [Required]
        public string Title { get; set; } = null!;

        public string? ImageUrl {  get; set; }
        public List<SongCheckboxViewModel>? Songs { get; set; }
    }
}
