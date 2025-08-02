using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MusicApp.Web.ViewModels.Admin.UserManagement
{
    public class UserManagementIndexViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
