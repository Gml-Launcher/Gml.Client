using Gml.Web.Api.Domains.System;

namespace GmlCore.Interfaces.Launcher
{
    public interface IStartupOptions
    {
        int MinimumRamMb { get; set; }
        int MaximumRamMb { get; set; }
        bool FullScreen { get; set; }
        int ScreenWidth { get; set; }
        int ScreenHeight { get; set; }
        string? ServerIp { get; set; }
        int ServerPort { get; set; }
        public string OsName { get; set; }
        public string OsArch { get; set; }
    }
}
