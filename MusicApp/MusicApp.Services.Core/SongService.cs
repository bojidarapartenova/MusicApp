using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;
using MusicApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using MusicApp.Web.ViewModels.Comment;
using System.ComponentModel;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using TagLib;
using System.Reflection.Metadata;

namespace MusicApp.Services.Core
{
    public class SongService : ISongService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly INotificationsService notificationService;
        public SongService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment, INotificationsService notificationService)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            this.notificationService = notificationService;
        }

        public async Task<IEnumerable<SongViewModel>> GetAllSongsAsync(string? searchTerm = null, int? genreId = null)
        {
            IEnumerable<SongViewModel> songs = await dbContext
                .Songs
                .AsNoTracking()
                .OrderByDescending(s=>s.ReleaseDate)
                .Select(s => new SongViewModel()
                {
                    Id = s.Id,
                    ImageUrl = s.ImageUrl ?? $"/images/no-image.jpg",
                    Title = s.Title,
                    Duration = s.Duration,
                    Artist = s.Artist,
                    Publisher=s.Publisher.UserName!,
                    Genre=s.Genre.Name,
                    GenreId=s.GenreId,
                    ReleaseDate=s.ReleaseDate,
                    PublisherId=s.PublisherId,
                    LikesCount=s.Likes
                })
                .ToArrayAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                songs = songs
                    .Where(s => s.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if(genreId.HasValue)
            {
                songs = songs
                    .Where(s => s.GenreId == genreId.Value);
            }

            return songs;
        }

        public async Task<bool> AddSongAsync(string userId, AddSongInputModel inputModel)
        {
            bool result = false;

            var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "audio");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(inputModel.AudioUrl.FileName);
            var filePath=Path.Combine(uploadsFolder, uniqueFileName);

            using(var stream =new FileStream(filePath, FileMode.Create))
            {
                await inputModel.AudioUrl.CopyToAsync(stream);
            }

            var tfile = TagLib.File.Create(filePath);
            var durationInSeconds = (int)tfile.Properties.Duration.TotalSeconds;

            ApplicationUser? user=await userManager.FindByIdAsync(userId);
            Genre? genre = await dbContext.Genres.FindAsync(inputModel.GenreId);

            if (user != null && genre!=null)
            {
                Song song = new Song()
                {
                    Title = inputModel.Title,
                    PublisherId = userId,
                    GenreId = inputModel.GenreId,
                    Artist = inputModel.Artist,
                    Duration = durationInSeconds,
                    ReleaseDate = DateTime.UtcNow,
                    Likes = 0,
                    ImageUrl = inputModel.ImageUrl?? $"/images/no-image.jpg",
                    AudioUrl = "/audio/"+uniqueFileName
                };

                await dbContext.Songs.AddAsync(song);
                await dbContext.SaveChangesAsync();

                result = true;
            }
            return result;
        }

        public async Task<EditSongInputModel?> GetSongToEditAsync(string userId, string? songId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            EditSongInputModel? songToEdit = null;

            bool isGuidValid = Guid.TryParse(songId, out Guid id);

            if (isGuidValid)
            {
                Song? song = await dbContext.Songs.FindAsync(id);

                if (song != null && song.PublisherId.ToLower()==userId.ToLower())
                {
                    songToEdit = new EditSongInputModel()
                    {
                        Id = song.Id,
                        Title = song.Title,
                        GenreId = song.GenreId,
                        Artist = song.Artist,
                        ImageUrl = song.ImageUrl,
                        AudioUrl = song.AudioUrl,
                        PublisherId=userId
                    };
                }
            }
            return songToEdit;
        }

        public async Task<bool> EditSongAsync(EditSongInputModel inputModel)
        {
            bool result = false;

            Genre? genre = await dbContext.Genres.FindAsync(inputModel.GenreId);
            Song? editedSong = await dbContext.Songs.FindAsync(inputModel.Id);

            if (genre != null && editedSong != null && editedSong.PublisherId.ToLower() == inputModel.PublisherId.ToLower())
            {
                editedSong.Title = inputModel.Title;
                editedSong.GenreId = inputModel.GenreId;
                editedSong.Artist = inputModel.Artist;
                editedSong.ImageUrl = inputModel.ImageUrl;

                if (inputModel.NewAudioFile != null)
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "audio");
                    Directory.CreateDirectory(uploadsFolder);

                    string newFileName = Guid.NewGuid() + Path.GetExtension(inputModel.NewAudioFile.FileName);
                    string newFilePath = Path.Combine(uploadsFolder, newFileName);

                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await inputModel.NewAudioFile.CopyToAsync(fileStream);
                    }

                    var tfile = TagLib.File.Create(newFilePath);
                    int durationInSeconds = (int)tfile.Properties.Duration.TotalSeconds;

                    string oldAudioPath = Path.Combine(webHostEnvironment.WebRootPath, editedSong.AudioUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldAudioPath))
                    {
                        System.IO.File.Delete(oldAudioPath);
                    }

                    editedSong.AudioUrl = "/audio/" + newFileName;
                    editedSong.Duration = durationInSeconds;
                }

                await dbContext.SaveChangesAsync();
                result = true;
            }

            return result;
        }


        public async Task<DeleteSongViewModel?> GetSongToDeleteAsync(string userId, string? songId)
        {
            DeleteSongViewModel? songToDelete=null;

            bool isGuidValid = Guid.TryParse(songId, out Guid id);

            if(songId!=null)
            {
                Song? song=await dbContext
                    .Songs
                    .Include(s=>s.Publisher)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(s=>s.Id==id);

                if(song!=null && song.PublisherId.ToLower()==userId.ToLower())
                {
                    songToDelete = new DeleteSongViewModel()
                    {
                        Id = song.Id,
                        Title = song.Title,
                        PublisherId=song.PublisherId,
                        Publisher = song.Publisher.UserName!
                    };
                }
            }
            return songToDelete;
        }

        public async Task<bool> SoftDeleteSongAsync(string userId, DeleteSongViewModel viewModel)
        {
            bool result = false;

            ApplicationUser? user=await userManager.FindByIdAsync(userId);
            Song? deletedSong=await dbContext.Songs.FindAsync(viewModel.Id);

            if(user!=null && deletedSong!=null &&
                deletedSong.PublisherId.ToLower()==userId.ToLower())
            {
                deletedSong.IsDeleted=true;

                await dbContext.SaveChangesAsync();
                result = true;
            }
            return result;
        }

        public async Task<SongViewModel?> GetSongByIdAsync(string songId)
        {
            SongViewModel? song = await dbContext.Songs
                .Select(s => new SongViewModel()
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist,
                    ImageUrl = s.ImageUrl,
                    Duration = s.Duration,
                    PublisherId = s.PublisherId,
                    Publisher=s.Publisher.UserName!,
                    ReleaseDate = s.ReleaseDate,
                    Genre = s.Genre.Name,
                    AudioUrl=s.AudioUrl,
                    LikesCount=s.Likes
                })
                .FirstOrDefaultAsync(s => s.Id.ToString() == songId);

            return song;
        }

        public async Task<bool> ToggleLikeAsync(string userId, Guid songId)
        {
            bool result = false;

            Song? song = await dbContext.Songs.FindAsync(songId);

            if (song != null)
            {
                Like? existingLike = await dbContext.Likes
                    .FirstOrDefaultAsync(l => l.User.Id == userId && l.SongId == songId);

                if (existingLike != null)
                {
                    dbContext.Likes.Remove(existingLike);
                    song.Likes-=1;

                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    Like like = new Like()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        SongId = songId
                    };

                    dbContext.Likes.Add(like);
                    song.Likes += 1;

                    if(song.PublisherId!=userId)
                    {
                        await notificationService.NotifySongLikedAsync(songId, userId);
                    }

                    await dbContext.SaveChangesAsync();
                    result = true;
                }
            }
            return result;
        }

        public async Task<int> GetSongLikeCountAsync(Guid songId)
        {
            int likes=await dbContext
                .Likes
                .CountAsync(l=>l.SongId == songId);

            return likes;
        }
    }
}
