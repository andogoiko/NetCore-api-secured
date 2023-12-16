using apiSecurizada.Models;
using apiSecurizada.Models.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using methods;

namespace apiSecurizada.Controllers

{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly ILogger<UsersController> _looger;
        private readonly IConfiguration _configuration;
        private readonly BBDDContext _dbContext;

        public UsersController(ILogger<UsersController> logger, IConfiguration configuration, BBDDContext dbContext)
        {
            _looger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet("my-name")]
        [Authorize]
        public ActionResult<string> MyUsername(){
            var claimUser = HttpContext.User;

            string claimUsername = claimUser.FindFirst("sub")?.Value;

            return claimUsername;
        }

        [HttpPost("register")]
        public ActionResult<User> Register(User request)
        {
            string passwordHash
                = BCrypt.Net.BCrypt.HashPassword(request.Password);

            User newUser = new User{
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Email = request.Email,
                Password = passwordHash,
                Role = request.Role
            };

            _dbContext.Users.Add(newUser);

            _dbContext.SaveChanges();


            return Ok();
        }

        [HttpPost("login")]
        public ActionResult<User> Login(string Username, string Password)
        {

            User user = _dbContext.Users.Where(u => u.Username == Username).FirstOrDefault();

            if (user.Username != Username)
            {
                return BadRequest("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                return BadRequest("Wrong password.");
            }

            string token = CreateToken(user);

            var refreshToken = GenerateRefreshToken(user);
            SetRefreshtoken(refreshToken, user);

            return Ok(token);
        }

        private string CreateToken(User user)
        {

            List<Claim> claims = new List<Claim>{
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Sub, user.Username),
                    new(JwtRegisteredClaimNames.Email, user.Email),
                    new("Role", user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireDate = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                    issuer : "https://id.ProbandoAuth.com",
                    audience : "https://swagger.ProbandoAuth.com",
                    claims: claims,
                    expires: expireDate,
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            var cookieOptions = new CookieOptions{
                HttpOnly = true, // La cookie solo es accesible desde el lado del servidor
                Secure = true,  // Solo se envía si la conexión es segura (HTTPS)
                SameSite = SameSiteMode.Strict, // More security options
                Expires = expireDate,
            };

            Response.Cookies.Append("AccessToken", jwt, cookieOptions);

            // saving on context the claims info to use the user on requests

            var identity = new ClaimsIdentity(claims, "User");
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync("User", principal);

            return jwt;
        }

        [HttpPost("update-access-token")]
        public async Task<ActionResult<string>> UpdateAcessToken(){

            var claimUser = HttpContext.User;

            string claimUsername = claimUser.FindFirst("email")?.Value;

            User user = _dbContext.Users.Where(u => u.Username == claimUsername).FirstOrDefault();

            if(user.RefreshToken.ExpireDate < DateTime.Now){

                return Unauthorized("Token expired.");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            SetRefreshtoken(newRefreshToken, user);

            return Ok();
        }

        private RefreshToken GenerateRefreshToken(User user){

            var refreshToken = new RefreshToken{
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreationDate = DateTime.Now,
                ExpireDate = DateTime.Now.AddDays(30),
                User = user
            };

            return refreshToken;
        }

        private void SetRefreshtoken(RefreshToken newrefreshToken, User user){


            RefreshToken rtoken = _dbContext.RefreshTokens.Where(rf => rf.UserId == user.Id).FirstOrDefault();

            if(rtoken == null){

                _dbContext.RefreshTokens.Add(newrefreshToken);

            }else{
                rtoken.Token = newrefreshToken.Token;
                rtoken.CreationDate = newrefreshToken.CreationDate;
                rtoken.ExpireDate = newrefreshToken.ExpireDate;

                _dbContext.Entry(rtoken).State = EntityState.Modified;
            }

            _dbContext.SaveChanges();

        }

    }

}