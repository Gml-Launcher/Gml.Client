using System.Diagnostics;
using Gml.Web.Api.Dto.Files;

namespace Gml.Client.Helpers;

public class ProfileFileWatcher
{
    private readonly string _directory;
    private List<ProfileFileReadDto> _allowedFiles;
    private Process _process;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly bool _needKill;
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

    public ProfileFileWatcher(string directory, List<ProfileFileReadDto> allowedFiles, Process process, bool needKill = true)
    {
        _directory = directory;
        _needKill = needKill;

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        _allowedFiles = allowedFiles;
        _process = process;
        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = directory,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        _fileSystemWatcher.Created += OnNewFileCreated;
    }


    private void OnNewFileCreated(object sender, FileSystemEventArgs e)
    {
        if (!e.FullPath.StartsWith(_directory) || _allowedFiles.Any(c => e.FullPath == Path.Combine(_directory, c.Directory))) return;

        FileAdded?.Invoke(this, e.FullPath);

        if (_needKill)
        {
            try
            {
                _process.Kill();
            }
            catch (InvalidOperationException ex)
            {
                // Ignore
            }
        }
    }
}
