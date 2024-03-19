namespace Gml.Web.Api.Dto.Integration;

public class IntegrationUpdateDto
{
    public int AuthType { get; set; }
    public string Endpoint { get; set; } = null!;
}