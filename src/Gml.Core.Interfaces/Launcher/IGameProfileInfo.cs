namespace GmlCore.Interfaces.Launcher
{
    public interface IGameProfileInfo
    {
        public string ProfileName { get; set; }
        public string MinecraftVersion { get; set; }
        public string ClientVersion { get; set; }
        public string Arguments { get; set; }
    }
}
