using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Song
{
    public class SongViewModel
    {
        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public string Title { get; set; } = null!;
        public int Duration {  get; set; }
        //public DateTime ReleaseDate { get; set; }
    }
}
