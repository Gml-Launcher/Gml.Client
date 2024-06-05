using System.IO;
using System.Threading.Tasks;
using GmlCore.Interfaces.System;
using Microsoft.AspNetCore.Http;

namespace GmlCore.Interfaces.Procedures
{
    public interface IFileStorageProcedures
    {
        Task<IFileInfo?> DownloadFileStream(string fileHash, Stream outputStream, IHeaderDictionary headers);
        Task<string> LoadFile(Stream fileStream);
    }
}
