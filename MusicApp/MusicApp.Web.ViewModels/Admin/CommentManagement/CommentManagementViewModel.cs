using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Admin.CommentManagement
{
    public class CommentManagementViewModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
