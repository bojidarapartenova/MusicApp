﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Comment
{
    public class DeleteCommentViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string PublisherId { get; set; } = null!;

        public Guid SongId {  get; set; }
    }
}
