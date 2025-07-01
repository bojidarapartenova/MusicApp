using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MusicApp.Data.Models
{
    public class ApplicationUser: IdentityUser
    {
        [MaxLength(50)]
        public string? Name { get; set; }
        public ICollection<Song> UploadedSongs { get; set; }=new HashSet<Song>();
        public ICollection<Playlist> Playlists { get; set; }=new HashSet<Playlist>();
        public ICollection<Notification> Notifications { get; set; }=new HashSet<Notification>();

    }
}
