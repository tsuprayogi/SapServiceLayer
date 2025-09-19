using Newtonsoft.Json;

public class LoginRequestDto
{
    [JsonProperty("Username")]
    public string Username { get; set; }

    [JsonProperty("Password")]
    public string Password { get; set; }
}