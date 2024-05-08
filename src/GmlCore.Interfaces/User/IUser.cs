using System;

namespace GmlCore.Interfaces.User
{
    public interface IUser
    {
        string Name { get; set; }
        string AccessToken { get; set; }
        string Uuid { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string TextureUrl { get; set; }
        public string ServerUuid { get; set; }
        public DateTime ServerExpiredDate { get; set; }
    }
}
