using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    //[AllowAnonymous]
    //[Authorize(Policy = IdentityData.AdminUserPolicyName)] //para validaciones extra, como que tenga que ser admin el que inicia sesion con el token
    /*Authorize]                                                //para validaciones extra, como que tenga que ser admin el que inicia sesion con el token
    [RequiresClaim(IdentityData.AdminUserClaimName, "true")]*/
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        try
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
        
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"Error en la generación del token: {ex.ToString()}");
                return null;
        }
        
    }
}
