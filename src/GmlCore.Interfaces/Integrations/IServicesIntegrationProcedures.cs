using System.Collections.Generic;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Auth;
using GmlCore.Interfaces.Enums;

namespace GmlCore.Interfaces.Integrations
{
    public interface IServicesIntegrationProcedures
    {
        Task<AuthType> GetAuthType();
        Task<IEnumerable<IAuthServiceInfo>> GetAuthServices();
        Task<IAuthServiceInfo?> GetActiveAuthService();
        Task<IAuthServiceInfo?> GetAuthService(AuthType authType);
        Task SetActiveAuthService(IAuthServiceInfo? service);
        Task<string> GetSkinServiceAsync();
        Task<string> GetCloakServiceAsync();
        Task SetSkinServiceAsync(string url);
        Task SetCloakServiceAsync(string url);
        Task<string?> GetSentryService();
        Task SetSentryService(string url);
        Task UpdateDiscordRpc(IDiscordRpcClient client);
        Task<IDiscordRpcClient?> GetDiscordRpc();
    }
}
