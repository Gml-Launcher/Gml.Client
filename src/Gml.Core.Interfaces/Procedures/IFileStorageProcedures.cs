using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GmlCore.Interfaces.System;
using Microsoft.AspNetCore.Http;

namespace GmlCore.Interfaces.Procedures
{
    public interface IFileStorageProcedures
    {
        Task<IFileInfo?> DownloadFileStream(string fileHash, Stream outputStream, IHeaderDictionary headers);
        Task<string> LoadFile(
            Stream fileStream,
            string? folder = null,
            string? defaultFileName = null,
            Dictionary<string, string>? tags = null);
        Task<(Stream File, string fileName, long Length)> GetFileStream(string fileHash, string? folder = null);
        Task<bool> CheckFileExists(string folder, string fileHash);
    }
}
