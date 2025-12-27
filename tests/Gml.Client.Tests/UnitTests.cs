using System.Diagnostics;
using Gml.Client.Interfaces;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;

namespace Gml.Client.Tests;

public class Tests
{
    private IGmlClientManager Client { get; set; }

    [TearDown]
    public void TearDown()
    {
        Client.Dispose();
    }

    [SetUp]
    public void Setup()
    {
        var localDirectory = Environment.CurrentDirectory;
        var baseAddress = "https://gmlb.recloud.tech";

        Client = new GmlClientManager(localDirectory, baseAddress, new GameLoader(), "GmlLauncher", OsType.Linux);

        Client.ProgressChanged.Subscribe(percentage =>
        {
            Console.WriteLine(percentage);
            Debug.WriteLine(percentage);
        });
    }

    [Test]
    public async Task GetClientsList()
    {
        var clients = await Client.GetProfiles();

        Assert.That(clients, Is.Not.Null);
    }

    [Test]
    public async Task GetProfileInfo()
    {
        // var clients = await Client.GetProfileInfo(new ProfileCreateInfoDto
        // {
        //     ClientName = "AztexCraft",
        //     GameAddress = "192.168.0.1",
        //     GamePort = 25565,
        //     RamSize = 4096,
        //     SizeX = 1500,
        //     SizeY = 900,
        //     IsFullScreen = false,
        //     UserAccessToken = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser",
        //     UserName = "GamerVII",
        //     UserUuid = "31f5f477-53db-4afd-b88d-2e01815f4887"
        // });
        //
        // Assert.That(clients, Is.Not.Null);
    }

    [Test]
    public async Task DownloadFiles()
    {
        // var localProfile = new ProfileCreateInfoDto
        // {
        //     ClientName = "AztexCraft",
        //     GameAddress = "207.180.231.31",
        //     GamePort = 25565,
        //     RamSize = 4096,
        //     SizeX = 1500,
        //     SizeY = 900,
        //     IsFullScreen = false,
        //     UserAccessToken = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser",
        //     UserName = "GamerVII",
        //     UserUuid = "31f5f477-53db-4afd-b88d-2e01815f4887"
        // };
        //
        // var profileInfo = await Client.GetProfileInfo(localProfile);
        //
        // await Client.DownloadNotInstalledFiles(profileInfo);
    }

    [Test]
    public async Task StartClient()
    {
        // var localProfile = new ProfileCreateInfoDto
        // {
        //     ProfileName = "1201",
        //     GameAddress = "207.180.231.31",
        //     GamePort = 25565,
        //     RamSize = 8192,
        //     IsFullScreen = false,
        //     OsType = ((int)OsType.Windows).ToString(),
        //     OsArchitecture = Environment.Is64BitOperatingSystem ? "64" : "32",
        //     UserAccessToken = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser",
        //     UserName = "GamerVII",
        //     UserUuid = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser"
        // };
        //
        // var profileInfo = await Client.GetProfileInfo(localProfile);
        //
        // if (profileInfo != null)
        // {
        //     await Client.DownloadNotInstalledFiles(profileInfo);
        //
        //     var process = await Client.GetProcess(profileInfo);
        //     var p = new ProcessUtil(process);
        //     p.OutputReceived += (s, e) => Console.WriteLine(e);
        //     p.StartWithEvents();
        //     await p.WaitForExitTaskAsync();
        //     // process.Start();
        // }
    }
}
