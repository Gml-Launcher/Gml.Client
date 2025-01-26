using GmlCore.Interfaces.Enums;

namespace GmlCore.Interfaces.Storage
{
    public interface IStorageSettings
    {
        public StorageType StorageType { get; set; }
        string StorageHost { get; set; }
        string StorageLogin { get; set; }
        string StoragePassword { get; set; }
        TextureProtocol TextureProtocol { get; set; }
    }
}
