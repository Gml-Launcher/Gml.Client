using System.Collections.Generic;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Storage;

namespace GmlCore.Interfaces.Launcher
{
    public interface ILauncherInfo
    {
        public string Name { get; }
        public string BaseDirectory { get; }
        public string InstallationDirectory { get; }
        public IStorageSettings StorageSettings { get; set; }
        Dictionary<string, IVersionFile?> ActualLauncherVersion { get; set; }
        IGmlSettings Settings { get; }
        void UpdateSettings(StorageType storageType, string storageHost, string storageLogin, string storagePassword,
            TextureProtocol textureProtocol);
        Task<IEnumerable<ILauncherBuild>> GetBuilds();
        Task<ILauncherBuild?> GetBuild(string name);
    }
}
