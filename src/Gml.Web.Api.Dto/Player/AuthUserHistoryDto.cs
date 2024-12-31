using System;

namespace Gml.Web.Api.Dto.Player;

public class AuthUserHistoryDto
{
    public DateTime Date { get; set; }
    public string Device { get; set; }
    public string? Address { get; set; }
    public string Protocol { get; set; }
    public string? Hwid { get; set; }
}
