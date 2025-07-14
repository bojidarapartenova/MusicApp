using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Song
{
    public class SongInPlaylistViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Artist { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }
}
