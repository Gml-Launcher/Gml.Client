using System.Reactive.Subjects;
using Gml.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Gml.Client.Helpers;

public class SignalRConnect : IDisposable, IAsyncDisposable
{
    private readonly ISubject<bool> _profilesChanged = new Subject<bool>();
    private readonly string _address;
    private readonly IUser _user;
    private HubConnection _hubConnection;
    public IObservable<bool> ProfilesChanges => _profilesChanged;

    public SignalRConnect(string address, IUser user)
    {
        _address = address;
        _user = user;
    }

    private string BuildHubUrl()
    {
        return $"{_address}?access_token={_user.AccessToken}";
    }

    public async Task BuildAndConnect()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(BuildHubUrl())
            .Build();
        await Connect();
        CreateListeners();
    }

    private async Task Connect()
    {
        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error starting connection: {ex.Message}");
        }
    }

    private void CreateListeners()
    {
        _hubConnection.Closed += async (error) =>
        {
            await Console.Error.WriteLineAsync($"[Gml.Protect] Connection closed: {error}");
        };

        _hubConnection.On("RequestLauncherHash", async () =>
        {
            await _hubConnection.SendAsync("ConfirmLauncherHash", "hash");
        });

        _hubConnection.On("RefreshProfiles", () =>
        {
            _profilesChanged.OnNext(true);
            return Task.CompletedTask;
        });

    }

    public async ValueTask DisposeAsync()
    {
        await _hubConnection.DisposeAsync();
    }

    public void Dispose()
    {
        _ = DisposeAsync();
    }

    public Task UpdateInfo()
    {
        return _hubConnection.SendAsync("UpdateUserLauncher", _user.Name);
    }
}
