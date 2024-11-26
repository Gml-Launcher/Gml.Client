using System.ComponentModel;
using System.Net;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Downloader;
using CmlLib.Core.Installer.Forge;

namespace CmlLib.Forge.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }


    [Test]
    public async Task Test2()
    {
        ServicePointManager.DefaultConnectionLimit = 256;

        var path = new MinecraftPath();
        var launcher = new CMLauncher(path);

        launcher.FileChanged += e =>
        {
            Console.WriteLine("FileKind: " + e.FileKind);
            Console.WriteLine("FileName: " + e.FileName);
            Console.WriteLine("ProgressedFileCount: " + e.ProgressedFileCount);
            Console.WriteLine("TotalFileCount: " + e.TotalFileCount);
        };
        launcher.ProgressChanged += (s, e) => { Console.WriteLine("{0}%", e.ProgressPercentage); };

        var versions = await launcher.GetAllVersionsAsync();
        foreach (var v in versions) Console.WriteLine(v.Name);

        var process = await launcher.CreateProcessAsync("1.19.2", new MLaunchOption
        {
            MaximumRamMb = 2048,
            Session = MSession.GetOfflineSession("hello123")
        });
    }

    [Test]
    public async Task Test1()
    {
        var path = new MinecraftPath(); // use default directory
        var launcher = new CMLauncher(path);
        launcher.FileChanged += fileChanged;
        launcher.ProgressChanged += progressChanged;

//Initialize MForge
        var forge = new MForge(launcher);
        forge.FileChanged += fileChanged;
        forge.ProgressChanged += progressChanged;
        forge.InstallerOutput += (s, e) => Console.WriteLine(e);

// Install the best forge for specific minecraft version
        var versionName = await forge.Install("1.20.1");

// Install with specific forge version
// var versionName = await forge.Install("1.20.1", "47.0.35");

//Start Minecraft
        var launchOption = new MLaunchOption
        {
            MaximumRamMb = 4096,
            ServerIp = "207.180.231.31",
            ServerPort = 25575,
            ScreenWidth = 1500,
            ScreenHeight = 900,
            Session = new MSession("GamerVII", "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser",
                "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser")
        };

        var process = await launcher.CreateProcessAsync(versionName, launchOption);
        process.Start();
    }

    private void fileChanged(DownloadFileChangedEventArgs e)
    {
        Console.WriteLine($"[{e.FileKind.ToString()}] {e.FileName} - {e.ProgressedFileCount}/{e.TotalFileCount}");
    }

    private void progressChanged(object? sender, ProgressChangedEventArgs e)
    {
        Console.WriteLine($"{e.ProgressPercentage}%");
    }
}
