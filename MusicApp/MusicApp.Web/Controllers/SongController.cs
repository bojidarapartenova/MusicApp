using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Comment;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.Controllers
{
    public class SongController : BaseController
    {
        private readonly ISongService songService;
        private readonly IGenreService genreService;
        private readonly IPlaylistsService playlistService;
        private readonly ICommentService commentService;
        public SongController(ISongService songService, IGenreService genreService, IPlaylistsService playlistsService, ICommentService commentService)
        {
            this.songService = songService;
            this.genreService = genreService;
            this.playlistService = playlistsService;
            this.commentService = commentService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IEnumerable<SongViewModel> songs = await songService.GetAllSongsAsync();

            return View(songs);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            try
            {
                AddSongInputModel inputModel = new AddSongInputModel
                {
                    Genres = await genreService.GetGenresDropDownAsync()
                };

                return View(inputModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddSongInputModel inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(inputModel);
                }
                bool result = await songService.AddSongAsync(GetUserId()!, inputModel);

                if (!result)
                {
                    return View(inputModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            try
            {
                string userId = GetUserId()!;
                EditSongInputModel? songToEdit = await
                    songService.GetSongToEditAsync(userId, id);

                if (songToEdit == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                songToEdit.Genres = await genreService.GetGenresDropDownAsync();

                return View(songToEdit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSongInputModel inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        return View(inputModel);
                    }
                }
                bool result = await songService.EditSongAsync(inputModel);

                if (result == false)
                {
                    return View(inputModel);
                }
                return RedirectToAction(nameof(Index), new { showModal = true, id = inputModel.Id });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            try
            {
                string userId = GetUserId()!;
                DeleteSongViewModel? songToDelete = await
                    songService.GetSongToDeleteAsync(userId, id);

                if (songToDelete == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return View(songToDelete);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteSongViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                bool result = await songService.SoftDeleteSongAsync(GetUserId()!, viewModel);

                if (result == false)
                {
                    return View(viewModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Listen(string id)
        {
            try
            {
                var userId = GetUserId();
                var song = await songService.GetSongByIdAsync(id);
                if (song == null)
                {
                    return RedirectToAction("Index");
                }

                if (userId != null)
                {
                    song.IsLiked = await playlistService.IsSongFavoritesAsync(userId, song.Id);
                }
                else
                {
                    song.IsLiked = false;
                }

                song.Comments = await commentService.GetCommentsAsync(song.Id, userId!);

                return View(song);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Like([FromBody] Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index));
                }
                string useId = GetUserId()!;

                bool isInFavorites = await playlistService.IsSongFavoritesAsync(useId, id);

                if (isInFavorites)
                {
                    await playlistService.RemoveSongFromFavoritesAsync(useId, id);
                }
                else
                {
                    await playlistService.AddSongToFavoritesAsync(useId, id);
                }

                return Json(new { liked = !isInFavorites });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(PostCommentInputModel inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Listen), new { id = inputModel.SongId });
                }

                string userId = GetUserId()!;
                await commentService.AddCommentAsync(inputModel, userId);

                return RedirectToAction(nameof(Listen), new { id = inputModel.SongId });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(DeleteCommentViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction("Listen", "Song", new { id = viewModel.SongId });
                }

                string userId = GetUserId()!;
                var comment = await commentService.GetCommentToDeleteAsync(userId, viewModel.Id);
                if(comment != null)
                {
                    bool result=await commentService.SoftDeleteCommentAsync(userId, comment);
                }

                return RedirectToAction("Listen", "Song", new { id = viewModel.SongId });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
