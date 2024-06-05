using System;

namespace GmlCore.Interfaces.User
{
    public interface IUser
    {
        string Name { get; set; }
        string AccessToken { get; set; }
        string Uuid { get; set; }
        public string? TextureSkinUrl { get; set; }
        public string ServerUuid { get; set; }
        bool IsBanned { get; set; }
        public DateTime ServerExpiredDate { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}
