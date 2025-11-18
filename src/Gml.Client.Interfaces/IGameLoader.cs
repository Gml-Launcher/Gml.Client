using Gml.Dto.Messages;
using Gml.Dto.Profile;

namespace Gml.Client.Interfaces;

public interface IGameLoader
{
    IGmlClientManager Manager { get; set; }
    Task StartGameAsync(ResponseMessage<ProfileReadInfoDto?>? profileInfo, bool isOnline);
}
