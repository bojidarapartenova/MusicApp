using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.ViewModels.Playlists
{
    public class PlaylistDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public List<SongViewModel> Songs { get; set; }=new List<SongViewModel>();
        public List<SongViewModel> AvailableSongs { get; set; } = new List<SongViewModel>();
    }
}
