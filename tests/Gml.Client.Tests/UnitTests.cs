using System.Diagnostics;
using Gml.WebApi.Models.Dtos.Profiles;
using Gml.WebApi.Models.Enums.System;

namespace Gml.Client.Tests;

public class Tests
{
    private IGmlClientManager Client { get; set; }

    [SetUp]
    public void Setup()
    {
        var localDirectory = "C:\\Users\\aa.terentiev\\AppData\\Roaming\\AztexClient";
        var baseAddress = "https://localhost:5000";

        Client = new GmlClientManager(baseAddress, localDirectory);

        Client.ProgressChanged += (sender, args) =>
        {
            Console.WriteLine(args.ProgressPercentage);
            Debug.WriteLine(args.ProgressPercentage);
        };
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
        var clients = await Client.GetProfileInfo(new ProfileCreateInfoDto
        {
            ClientName = "AztexCraft",
            GameAddress = "192.168.0.1",
            GamePort = 25565,
            RamSize = 4096,
            SizeX = 1500,
            SizeY = 900,
            IsFullScreen = false,
            UserAccessToken = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser",
            UserName = "GamerVII",
            UserUuid = "31f5f477-53db-4afd-b88d-2e01815f4887"
        });

        Assert.That(clients, Is.Not.Null);
    }

    [Test]
    public async Task DownloadFiles()
    {
        var localProfile = new ProfileCreateInfoDto
        {
            ClientName = "AztexCraft",
            GameAddress = "207.180.231.31",
            GamePort = 25565,
            RamSize = 4096,
            SizeX = 1500,
            SizeY = 900,
            IsFullScreen = false,
            UserAccessToken = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser",
            UserName = "GamerVII",
            UserUuid = "31f5f477-53db-4afd-b88d-2e01815f4887"
        };

        var profileInfo = await Client.GetProfileInfo(localProfile);

        await Client.DownloadNotInstalledFiles(profileInfo);
    }

    [Test]
    public async Task StartClient()
    {
        var localProfile = new ProfileCreateInfoDto
        {
            ClientName = "Hitech",
            GameAddress = "207.180.231.31",
            GamePort = 25565,
            RamSize = 4096,
            IsFullScreen = false,
            OsType = (int)OsType.Windows,
            OsArchitecture = Environment.Is64BitOperatingSystem ? "64" : "32",
            UserAccessToken = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser",
            UserName = "GamerVII",
            UserUuid = "sergsecgrfsecgriseuhcygrshecngrysicugrbn7csewgrfcsercgser"
        };

        var profileInfo = await Client.GetProfileInfo(localProfile);

        if (profileInfo != null)
        {
            await Client.DownloadNotInstalledFiles(profileInfo);

            var process = await Client.GetProcess(profileInfo);

            process.Start();
        }

    }
}
