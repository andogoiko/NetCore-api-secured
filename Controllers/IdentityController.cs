using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Security.Cryptography;

[Route("token")]
    [ApiController]
    public class IdentityController : ControllerBase
    {

        private const string TokenSecret = "Warningestonoesnadasegurocambialo";
        public static User user = new User();
        private static IConfigurationRoot conf = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly TimeSpan TokenLifeTime = TimeSpan.FromMinutes(5);

        //[EnableCors("MyPolicy")]
        //[Consumes("application/json")]
        [HttpPost, Route("generate-token")]
        public IActionResult GenerateToken([FromBody]TokenGenerationRequest request)
        {
           var jwt = CreateToken(request);

           return Ok(jwt);
        }

       

        private string CreateToken(TokenGenerationRequest request){
            
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

           return jwt;
        }

        
    }