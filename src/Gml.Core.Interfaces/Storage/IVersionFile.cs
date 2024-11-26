using System;

namespace GmlCore.Interfaces.Storage;

public interface IVersionFile : ICloneable
{
    string Version { get; set; }
    string Title { get; set; }
    string Description { get; set; }
    string Guid { get; set; }
}
