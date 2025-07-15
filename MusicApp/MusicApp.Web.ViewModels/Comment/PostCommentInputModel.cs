using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Comment
{
    public class PostCommentInputModel
    {
        public Guid SongId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Text { get; set; } = null!;
    }
}
