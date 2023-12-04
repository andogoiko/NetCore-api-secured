using apiSecurizada.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace apiSecurizada.Controllers

{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpGet, Authorize]
        public ActionResult<string> GetMyName()
        {
            return Ok(_userService.GetMyName());

        }

        [HttpPost("register")]
        public ActionResult<User> Register(UserDTO request)
        {
            string passwordHash
                = BCrypt.Net.BCrypt.HashPassword(request.Password);

            user.Username = request.Username;
            user.PasswordHash = passwordHash;

            return Ok(user);
        }

        [HttpPost("login")]
        public ActionResult<User> Login(UserDTO request)
        {
            if (user.Username != request.Username)
            {
                return BadRequest("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }

            string token = CreateToken(user);

            var refreshToken = GenerateRefreshToken();
            SetRefreshtoken(refreshToken);

            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken(){

            var refreshToken = Request.Cookies["refreshToken"];

            if(!user.RefreshToken.Equals(refreshToken)){
                
                return Unauthorized("Invalid refresh token.");
            }else if(user.TokenExpires < DateTime.Now){

                return Unauthorized("Token expired.");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            SetRefreshtoken(newRefreshToken);

            return Ok(token);
        }

        private string CreateToken(User user)
        {
            // List<Claim> claims = new List<Claim> {
            //     new Claim(ClaimTypes.Name, user.Username),
            //     new Claim(ClaimTypes.Role, "Admin"),
            //     new Claim(ClaimTypes.Role, "User"),
            // };

            List<Claim> claims = new List<Claim>{
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Sub, user.Username),
                    new(JwtRegisteredClaimNames.Email, user.Email),
                    new("isAdmin", user.IsAdmin.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    issuer : "https://id.ProbandoAuth.com",
                    audience : "https://swagger.ProbandoAuth.com",
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private RefreshToken GenerateRefreshToken(){

            var refreshToken = new RefreshToken{
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7)
            };

            return refreshToken;
        }

        private void SetRefreshtoken(RefreshToken newrefreshToken){

            var cookieOptions = new CookieOptions{
                HttpOnly = true, // La cookie solo es accesible desde el lado del servidor
                Secure = true,  // Solo se envía si la conexión es segura (HTTPS)
                SameSite = SameSiteMode.None, // Puedes ajustar esto según tus necesidades
                Expires = newrefreshToken.Expires,
            };

            Response.Cookies.Append("refreshToken", newrefreshToken.Token, cookieOptions);

            user.RefreshToken = newrefreshToken.Token;
            user.TokenCreated = newrefreshToken.Created;
            user.TokenExpires = newrefreshToken.Expires;
        }

    }

}