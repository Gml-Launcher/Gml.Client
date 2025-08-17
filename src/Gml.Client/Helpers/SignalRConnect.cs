using System.Diagnostics;
using System.Reactive.Subjects;
using Gml.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Gml.Client.Helpers;

public class SignalRConnect : IDisposable, IAsyncDisposable
{
    private readonly string _address;
    private readonly ISubject<bool> _profilesChanged = new Subject<bool>();
    private readonly IUser _user;
    private HubConnection _hubConnection;

    public SignalRConnect(string address, IUser user)
    {
        _address = address;
        _user = user;
    }

    public IObservable<bool> ProfilesChanges => _profilesChanged;

    public async ValueTask DisposeAsync()
    {
        await _hubConnection.DisposeAsync();
    }

    public void Dispose()
    {
        _ = DisposeAsync();
    }

    private string BuildHubUrl()
    {
        return $"{_address}?access_token={_user.AccessToken}";
    }

    public async Task BuildAndConnect()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(BuildHubUrl())
            .WithAutomaticReconnect()
            .Build();
        await Connect();
        CreateListeners();
    }

    private async Task Connect()
    {
        try
        {
            await _hubConnection.StartAsync();
            Debug.WriteLine("SignalR connection established");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting connection: {ex}");
        }
    }

    private void CreateListeners()
    {
        _hubConnection.Closed += async error =>
        {
            await Console.Error.WriteLineAsync($"[Gml.Protect] Connection closed: {error}");
        };
https://gmlf.nazzy.team/
        _hubConnection.On("RequestLauncherHash",
            async () => { await _hubConnection.SendAsync("ConfirmLauncherHash", "hash"); });

        _hubConnection.On("RefreshProfiles", () =>
        {
            _profilesChanged.OnNext(true);
            return Task.CompletedTask;
        });
    }

    public Task UpdateInfo()
    {
        return _hubConnection.SendAsync("UpdateUserLauncher", _user.Name);
    }
}
