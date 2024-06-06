using System.IO;

namespace GmlCore.Interfaces.System
{
    public interface IFileInfo
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
        public string FullPath { get; set; }
    }
}
