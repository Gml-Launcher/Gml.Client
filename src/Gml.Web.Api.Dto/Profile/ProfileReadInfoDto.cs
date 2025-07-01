using System.Collections.Generic;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Player;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.System;

namespace Gml.Web.Api.Dto.Profile;

public class ProfileReadInfoDto
{
    public string JavaPath { get; set; }
    public string ProfileName { get; set; }
    public string DisplayName { get; set; }
    public string MinecraftVersion { get; set; }
    public string ClientVersion { get; set; }
    public string LaunchVersion { get; set; }
    public string IconBase64 { get; set; }
    public string Description { get; set; }
    public string Arguments { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public int RecommendedRam { get; set; }
    public string JvmArguments { get; set; }
    public string GameArguments { get; set; }
    public bool HasUpdate { get; set; }
    public ProfileState State { get; set; }
    public List<ProfileFileReadDto> Files { get; set; }
    public List<ProfileFolderReadDto> WhiteListFolders { get; set; }
    public List<ProfileFileReadDto> WhiteListFiles { get; set; }
    public List<PlayerReadDto> UsersWhiteList { get; set; }
    public string Background { get; set; }
}
