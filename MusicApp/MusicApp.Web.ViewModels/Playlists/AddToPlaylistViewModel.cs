using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MusicApp.Web.ViewModels.Playlists
{
    public class AddToPlaylistViewModel
    {
        public string SongId { get; set; } = null!;
        public string SelectedPlaylistId { get; set; } = null!;
        public List<SelectListItem> Playlists { get; set; }=null!;
    }
}
