using System.Diagnostics;
using Gml.Dto.Files;

namespace Gml.Client.Helpers;

public class ProfileFileWatcher
{
    private readonly string _directory;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly bool _needKill;

    public ProfileFileWatcher(string directory, List<ProfileFileReadDto> allowedFiles, Process process,
        bool needKill = true)
    {
        _directory = directory;
        _needKill = needKill;

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        AllowFiles = allowedFiles;
        Process = process;
        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = directory,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        _fileSystemWatcher.Created += OnNewFileCreated;
    }

    public Process Process { get; set; }

    public List<ProfileFileReadDto> AllowFiles { get; set; }

    internal event EventHandler<string>? FileAdded;


    private void OnNewFileCreated(object sender, FileSystemEventArgs e)
    {
        if (!e.FullPath.StartsWith(_directory) ||
            AllowFiles.Any(c => e.FullPath == Path.Combine(_directory, c.Directory))) return;

        FileAdded?.Invoke(this, e.FullPath);

        if (_needKill)
            try
            {
                Process.Kill();
            }
            catch (InvalidOperationException ex)
            {
                // Ignore
            }
    }
}
