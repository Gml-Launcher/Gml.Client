using System.Diagnostics;
using Gml.Client.Interfaces;
using Gml.Dto.Messages;
using Gml.Dto.Profile;

namespace Gml.Client;

public interface IGameLoader
{
    IGmlClientManager Manager { get; set; }
    IObservable<Process> GameLaunched { get; }
    Task StartGameAsync(ResponseMessage<ProfileReadInfoDto?>? profileInfo, bool isOnline);
    void ForceStop();
}
