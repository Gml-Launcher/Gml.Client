using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;

namespace GmlCore.Interfaces.Auth
{
    public interface IAuthServiceInfo
    {
        public string Name { get; set; }
        public AuthType AuthType { get; set; }
        string Endpoint { get; set; }
    }
}
