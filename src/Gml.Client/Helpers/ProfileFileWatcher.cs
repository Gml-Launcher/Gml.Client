using System.Diagnostics;
using Gml.Web.Api.Dto.Files;

namespace Gml.Client.Helpers;

public class ProfileFileWatcher
{
    private readonly string _directory;
    private List<ProfileFileReadDto> _allowedFiles;
    private Process _process;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly string _modsPath;
    internal event EventHandler<string>? FileAdded;

    public Process Process
    {
        get => _process;
        set => _process = value;
    }

    public List<ProfileFileReadDto> AllowFiles
    {
        get => _allowedFiles;
        set => _allowedFiles = value;
    }

    public ProfileFileWatcher(string directory, List<ProfileFileReadDto> allowedFiles, Process process)
    {
        _directory = directory;

        _modsPath = Path.Combine(directory, "mods");

        if (!Directory.Exists(_modsPath))
        {
            Directory.CreateDirectory(_modsPath);
        }
        _allowedFiles = allowedFiles;
        _process = process;
        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = _modsPath,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
        };

        _fileSystemWatcher.Created += OnNewFileCreated;
    }


    private void OnNewFileCreated(object sender, FileSystemEventArgs e)
    {
        if (!e.FullPath.StartsWith(_modsPath) || _allowedFiles.Any(c => e.FullPath == Path.Combine(_directory, c.Directory))) return;

        FileAdded?.Invoke(this, e.FullPath);

        _process.Kill();
    }
}
