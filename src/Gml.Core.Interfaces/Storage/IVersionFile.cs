using System.IO;
using System.Text.Json.Serialization;

namespace GmlCore.Interfaces.Storage;

public interface IVersionFile
{
    string Version { get; set; }
    string Title { get; set; }
    string Description { get; set; }
    [JsonIgnore] Stream? File { get; set; }
    string Guid { get; set; }
}
