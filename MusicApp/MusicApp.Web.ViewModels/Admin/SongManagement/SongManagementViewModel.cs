using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Admin.SongManagement
{
    public class SongManagementViewModel
    {
        public Guid Id { get; set; } 
        public string Title { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public string Publisher { get; set; } = null!;
        public string ReleaseDate { get; set; } = null!;
        public bool IsDeleted {  get; set; }
    }
}
