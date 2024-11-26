namespace GmlCore.Interfaces.Versions;

public interface IVersion
{
    string Name { get; set; }
    bool IsRelease { get; set; }
}
