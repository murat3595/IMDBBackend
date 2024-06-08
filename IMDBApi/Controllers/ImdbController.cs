using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IMDBApi;
using IMDBApi.Data;
using IMDBApi.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static TuitionApi.Controllers.V1.ImdbController;

namespace TuitionApi.Controllers.V1
{
    [Route("api/Imdb")]
    [ApiController]
    public class ImdbController : ControllerBase
    {
        private IConfiguration _config;
        private ImdbDbContext imdbDbContext;
        private CurrentUserService currentUserService;  


        public ImdbController(ImdbDbContext imdbDbContext, IConfiguration config, CurrentUserService currentUserService)
        {
            this.imdbDbContext = imdbDbContext;
            this._config = config;
            this.currentUserService = currentUserService;
        }

        public class UserLoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpGet("GetMoviesOrActors")]
        public IActionResult GetMoviesOrActors([FromQuery] string? search, [FromQuery] int mtype, [FromQuery] int count, [FromQuery] int pageIndex)
        {
            var userId = currentUserService.User?.Id;
            var result = new
            {
                things = imdbDbContext.MovieActor
                .Where(s => (search == null || s.Name.ToLower()
                .Contains(search.ToLower())) && (mtype == -1 || s.MType == mtype))
                .OrderByDescending(s => s.Rating)
                .Skip(pageIndex * count)
                .Take(count)
                .ToList()
            };

            foreach (var item in result.things)
            {
                item.AddedToWatchlist = imdbDbContext.UserWatchList.Any(s => s.UserId == userId && s.MovieId == item.Id);
                item.Rate = imdbDbContext.MovieComment.FirstOrDefault(s => s.UserId == userId && s.MovieId == item.Id);
            }

            return Ok(result);
        }

        public class WatchListRequest
        {
            public int MovieId { get; set; }
        }

        [Authorize]
        [HttpPost("AddToWatchList")]
        public IActionResult AddToWatchList([FromBody] WatchListRequest request)
        {
            var userId = currentUserService.User.Id;
            if (imdbDbContext.UserWatchList.Any(s => s.MovieId == request.MovieId && s.UserId == userId))
            {
                return Ok();
            }

            imdbDbContext.UserWatchList.Add(new UserWatchList
            {
                MovieId = request.MovieId,
                UserId = userId
            });

            imdbDbContext.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpPost("RemoveFromWatchlist")]
        public IActionResult RemoveFromWatchlist([FromBody] WatchListRequest request)
        {
            var userId = currentUserService.User?.Id;


            var listelement = imdbDbContext.UserWatchList.FirstOrDefault(s => s.UserId == userId && s.MovieId == request.MovieId);

            if (listelement != null)
            {
                imdbDbContext.UserWatchList.Remove(listelement);
                imdbDbContext.SaveChanges();
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("GetWatchlist")]
        public IActionResult GetWatchlist()
        {
            var userId = currentUserService.User?.Id;

            var list = imdbDbContext.UserWatchList.Where(s => s.UserId == userId).Select(k => k.MovieId).ToList();

            return Ok(new
            {
                watchlist = imdbDbContext.MovieActor.Where(s => list.Contains(s.Id)).ToList()
            });
        }

        public class RateRequest
        {
            public int MovieId { get; set; }
            public double Rate { get; set; }
            public string Comment { get; set; } 
        }

        [Authorize]
        [HttpPost("Rate")]
        public IActionResult Rate([FromBody] RateRequest request)
        {
            var userId = currentUserService.User.Id;

            var rate = imdbDbContext.MovieComment.Where(s => s.MovieId == request.MovieId && s.UserId == userId).FirstOrDefault();

            if (rate == null)
            {
                rate = new MovieComment
                {
                    Comment = request.Comment,
                    MovieId = request.MovieId,
                    UserId = userId,
                    Point = request.Rate
                };

                imdbDbContext.MovieComment.Add(rate);
            }
            else
            {
                rate.Point = request.Rate;
                rate.Comment = request.Comment;
            }
            imdbDbContext.SaveChanges();

            return Ok();
        }
    }
}