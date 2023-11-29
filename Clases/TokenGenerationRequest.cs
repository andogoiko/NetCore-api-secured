using System.Text.Json;

public class TokenGenerationRequest
{
    public string Email { get; set; }
    public int UserId { get; set; }
    public Dictionary<string, JsonElement> CustomClaims { get; set; }
}