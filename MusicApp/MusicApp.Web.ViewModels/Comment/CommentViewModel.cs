using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Comment
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Text { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public bool IsOwner { get; set; }
    }
}
