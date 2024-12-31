using System;
using Newtonsoft.Json;

namespace Gml.Web.Api.Domains.Integrations;

public class UnicoreAuthResult
{
    [JsonProperty("user")]
    public User User { get; set; }

    [JsonProperty("accessToken")]
    public string AccessToken { get; set; }

    [JsonProperty("refreshToken")]
    public string RefreshToken { get; set; }
}

public class Ban
{
    [JsonProperty("reason")]
    public string Reason { get; set; }

    [JsonProperty("expires")]
    public object Expires { get; set; }

    [JsonProperty("created")]
    public DateTime Created { get; set; }
}

public class User
{
    [JsonProperty("uuid")]
    public string Uuid { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("activated")]
    public bool Activated { get; set; }

    [JsonProperty("accessToken")]
    public object AccessToken { get; set; }

    [JsonProperty("serverId")]
    public object ServerId { get; set; }

    [JsonProperty("two_factor_enabled")]
    public object TwoFactorEnabled { get; set; }

    [JsonProperty("two_factor_secret")]
    public object TwoFactorSecret { get; set; }

    [JsonProperty("two_factor_secret_temp")]
    public string TwoFactorSecretTemp { get; set; }

    [JsonProperty("real")]
    public int Real { get; set; }

    [JsonProperty("virtual")]
    public int Virtual { get; set; }

    [JsonProperty("perms")]
    public object Perms { get; set; }

    [JsonProperty("created")]
    public DateTime Created { get; set; }

    [JsonProperty("updated")]
    public DateTime Updated { get; set; }

    [JsonProperty("cloak")]
    public object Cloak { get; set; }

    [JsonProperty("ban")]
    public Ban Ban { get; set; }
}
