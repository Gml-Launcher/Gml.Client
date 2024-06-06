#nullable enable
namespace Gml.Web.Api.Domains.Integrations;

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string? Login { get; set; }
    public string? Uuid { get; set; }
}
