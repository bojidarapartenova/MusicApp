using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Playlists
{
    public class RemoveSongViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
    }
}
