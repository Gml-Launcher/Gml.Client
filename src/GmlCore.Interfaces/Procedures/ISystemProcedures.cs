namespace GmlCore.Interfaces.Procedures
{
    public interface ISystemProcedures
    {
        public string DefaultInstallation { get; }

        string CleanFolderName(string name);

        string GetDefaultInstallationPath();
    }
}
