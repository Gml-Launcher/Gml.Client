using System.Threading.Tasks;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Procedures
{
    public interface IUserProcedures
    {
        Task<IUser> GetAuthData(string login, string password, string device);
        Task<IUser?> GetUserByUuid(string uuid);
        Task<IUser?> GetUserByName(string userName);
        Task<bool> ValidateUser(string userUuid, string uuid, string accessToken);
        Task<bool> CanJoinToServer(IUser user, string serverId);
    }
}
