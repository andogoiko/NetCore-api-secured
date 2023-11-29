using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

[Route("api/[controller]")]
    [ApiController]
    public class Identityontroller : ControllerBase
    {

        private const string TokenSecret = "Warningestonoesnadasegurocambialo";
        private static readonly TimeSpan TokenLifeTime = TimeSpan.FromHours(8);

        //[EnableCors("MyPolicy")]
        //[Consumes("application/json")]
        [HttpPost, Route("token")]
        public IActionResult GenerateToken([FromBody]TokenGenerationRequest request)
        {
           var tokenHandler = new JwtSecurityTokenHandler();
           var key = Encoding.UTF8.GetBytes(TokenSecret);

           var claims = new List<Claim>{
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, request.Email),
                new(JwtRegisteredClaimNames.Email, request.Email),
                new("userid", request.UserId.ToString())
           };

           foreach (var claimPair in request.CustomClaims)
           {
                var jsonElement = (JsonElement)claimPair.Value;
                var valueType = jsonElement.ValueKind switch
                {
                    JsonValueKind.True => ClaimValueTypes.Boolean,
                    JsonValueKind.False => ClaimValueTypes.Boolean,
                    JsonValueKind.Number => ClaimValueTypes.Double,
                    _ => ClaimValueTypes.String
                };
           }

           var tokenDescriptor = new SecurityTokenDescriptor
           {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TokenLifeTime),
            Issuer = "https://id.ProbandoAuth.com",
            Audience = "https://swagger.ProbandoAuth.com",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
           };

           var token = tokenHandler.CreateToken(tokenDescriptor);

           var jwt = tokenHandler.WriteToken(token);

           return Ok(jwt);
        }
    }