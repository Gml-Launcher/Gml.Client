using System;
using Newtonsoft.Json;

namespace Gml.Web.Api.Domains.Integrations;

public class AzuriomAuthResult
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonProperty("money")]
    public int Money { get; set; }

    [JsonProperty("banned")]
    public bool Banned { get; set; }

    [JsonProperty("uuid")]
    public string Uuid { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
}
