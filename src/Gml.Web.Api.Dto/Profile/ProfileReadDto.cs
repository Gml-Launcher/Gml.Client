using System;
using System.Collections.Generic;
using Gml.Web.Api.Dto.Servers;
using GmlCore.Interfaces.Enums;

namespace Gml.Web.Api.Dto.Profile;

public class ProfileReadDto
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string Description { get; set; }
    public string GameVersion { get; set; }
    public string LaunchVersion { get; set; }
    public string IconBase64 { get; set; }
    public string Background { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public int RecommendedRam { get; set; }
    public string JvmArguments { get; set; }
    public string GameArguments { get; set; }
    public ProfileState State { get; set; }
    public int Loader { get; set; }
    public List<ServerReadDto> Servers { get; set; } = [];
}
