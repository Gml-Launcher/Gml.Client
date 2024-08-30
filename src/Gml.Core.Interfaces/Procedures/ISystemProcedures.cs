using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GmlCore.Interfaces.Bootstrap;
using GmlCore.Interfaces.Launcher;

namespace GmlCore.Interfaces.Procedures
{
    public interface ISystemProcedures
    {
        public string DefaultInstallation { get; }
        string? BuildDotnetPath { get; }

        string CleanFolderName(string name);

        string GetDefaultInstallationPath();
        Task<bool> InstallDotnet();
        Task<string> GetAvailableMirrorAsync(IDictionary<string, string[]> mirrorUrls);
        Task DownloadFileAsync(string url, string destinationFilePath);
        void ExtractZipFile(string zipFilePath, string extractPath);
        void SetFileExecutable(string filePath);
        Task<IEnumerable<IBootstrapProgram>> GetJavaVersions();
    }
}
