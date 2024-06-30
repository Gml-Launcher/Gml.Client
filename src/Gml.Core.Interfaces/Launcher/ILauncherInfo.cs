using System.Collections.Generic;
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
        Dictionary<OsType, IVersionFile?> ActualLauncherVersion { get; set; }
        void UpdateSettings(StorageType storageType, string storageHost, string storageLogin, string storagePassword);
    }
}
