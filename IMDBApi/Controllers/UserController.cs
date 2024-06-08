using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IMDBApi.Data;
using IMDBApi.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using IMDBApi;

namespace TuitionApi.Controllers.V1
{
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IConfiguration _config;
        private ImdbDbContext imdbDbContext;
        private CurrentUserService currentUserService;

        public UserController(ImdbDbContext imdbDbContext, IConfiguration config, CurrentUserService currentUserService)
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

        public class GoogleAuthRequest
        {
            public string idToken { get; set; }
        }

        [HttpPost("GoogleLogin")]
        public IActionResult LoginWithGoogle([FromBody] GoogleAuthRequest request)
        {
            var user = GoogleAuth(request.idToken).Result;

            if (user != null)
            {
                var token = Generate(user);
                return Ok(new
                {
                    token = token
                });
            }

            return NotFound();
        }


        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLoginRequest userLogin)
        {
            var user = Authenticate(userLogin);

            if (user != null)
            {
                var token = Generate(user);
                return Ok(new
                {
                    token = token
                });
            }

            return NotFound();
        }

        [HttpGet("CurrentUser")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                user = (User?)currentUserService.User
            });
        }

        public class RegisterRequest : UserLoginRequest
        {
            public string Username { get; set; }
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            var user = RegisterUser(request);

            if (user != null)
            {
                var token = Generate(user);
                return Ok(new
                {
                    token = token
                });
            }

            return NotFound();
        }


        private string Generate(User user)
        {
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "_?"));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey,
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddMinutes(15),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private User Authenticate(UserLoginRequest userLogin)
        {
            var currentUser = imdbDbContext.Users
                .FirstOrDefault(o => o.Email.ToLower() == userLogin.Email.ToLower() && o.Password == userLogin.Password);

            if (currentUser != null)
            {
                return currentUser;
            }

            return null;
        }

        private async Task<User> GoogleAuth(string idToken)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            var email = payload.Email;

            var currentUser = imdbDbContext.Users
                .FirstOrDefault(o => o.Email.ToLower() == email);

            if (currentUser == null)
            {
                currentUser = new User
                {
                    Email = email,
                    Password = "",
                    Username = payload.GivenName + " " + payload.FamilyName,
                };
                imdbDbContext.Set<User>().Add(currentUser);
                imdbDbContext.SaveChanges();
            }

            return currentUser;
        }

        private User RegisterUser(RegisterRequest registerRequest)
        {
            var user = new IMDBApi.Data.Models.User
            {
                Email = registerRequest.Email.ToLower(),
                Password = registerRequest.Password,
                Username = registerRequest.Username,
            };
            imdbDbContext.Users.Add(user);

            imdbDbContext.SaveChanges();

            return user;
        }
    }
}