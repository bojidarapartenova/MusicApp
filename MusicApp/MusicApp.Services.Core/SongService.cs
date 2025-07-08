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

namespace MusicApp.Services.Core
{
    public class SongService : ISongService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        public SongService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<IEnumerable<SongViewModel>> GetAllSongsAsync()
        {
            IEnumerable<SongViewModel> songs = await dbContext
                .Songs
                .AsNoTracking()
                .Select(s => new SongViewModel()
                {
                    Id = s.Id,
                    ImageUrl = s.ImageUrl ?? $"/images/no-image.jpg",
                    Title = s.Title,
                    Duration = s.Duration,
                    Artist = s.Artist,
                    Publisher=s.Publisher.UserName!,
                    Genre=s.Genre.Name,
                    ReleaseDate=s.ReleaseDate,
                    PublisherId=s.PublisherId
                })
                .ToArrayAsync();

            return songs;
        }

        public async Task<bool> AddSongAsync(string userId, AddSongInputModel inputModel)
        {
            bool result = false;

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
                    Duration = inputModel.Duration,
                    ReleaseDate = DateTime.UtcNow,
                    Likes = 0,
                    ImageUrl = inputModel.ImageUrl?? $"/images/no-image.jpg",
                    AudioUrl = inputModel.AudioUrl
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
                        Duration = song.Duration,
                        Likes= song.Likes,
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

            Genre? genre=await dbContext.Genres.FindAsync(inputModel.GenreId);
            Song? editedSong = await dbContext.Songs.FindAsync(inputModel.Id);

            if(genre!=null && editedSong!=null)
            {
                editedSong.Title=inputModel.Title;
                editedSong.GenreId=inputModel.GenreId;
                editedSong.Artist=inputModel.Artist;
                editedSong.Duration=inputModel.Duration;
                editedSong.Likes=inputModel.Likes;
                editedSong.ImageUrl=inputModel.ImageUrl;
                editedSong.AudioUrl=inputModel.AudioUrl;
                editedSong.PublisherId=inputModel.PublisherId;

                await dbContext.SaveChangesAsync();
                result= true;
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
                    AudioUrl=s.AudioUrl
                })
                .FirstOrDefaultAsync(s => s.Id.ToString() == songId);

            return song;
        }
    }
}
