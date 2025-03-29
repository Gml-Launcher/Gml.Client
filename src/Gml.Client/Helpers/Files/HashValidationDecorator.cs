using System.Collections.Concurrent;
using System.Security.Cryptography;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Profile;

namespace Gml.Client.Helpers.Files;

public class HashValidationDecorator : IFileUpdateHandler
{
    private readonly IFileUpdateHandler _handler;

    public HashValidationDecorator(IFileUpdateHandler handler)
    {
        _handler = handler;
    }

    public async Task<FileValidationResult> ValidateFilesAsync(ProfileReadInfoDto profileInfo, string rootDirectory)
    {
        var result = await _handler.ValidateFilesAsync(profileInfo, rootDirectory);
        var filesToUpdate = new ConcurrentBag<ProfileFileReadDto>();
        var filesToDelete = new ConcurrentDictionary<string, ProfileFileReadDto>(
            result.FilesToDelete.ToDictionary(
                f => NormalizePath(f.Directory),
                f => f
            )
        );
        var files = result.FilesToUpdate.ToList();

        await Task.WhenAll(files.Select(async serverFile =>
        {

            filesToDelete.TryGetValue(NormalizePath(serverFile.Directory), out var localFile);

            if (localFile is null)
            {
                var localPath = Path.Combine(rootDirectory, serverFile.Directory.TrimStart('\\'));

                if (File.Exists(localPath))
                {
                    var fileInfo = new FileInfo(localPath);
                    // Сначала проверяем размер
                    if (fileInfo.Length == serverFile.Size)
                    {
                        // Размеры совпадают - проверяем хеш
                        localFile = new ProfileFileReadDto
                        {
                            Name = serverFile.Name,
                            Directory = serverFile.Directory,
                            Size = fileInfo.Length,
                        };
                    }
                    else
                    {
                        // Размеры не совпадают - сразу добавляем в список на обновление
                        filesToUpdate.Add(serverFile);
                        return;
                    }
                }
            }

            if (localFile == null)
            {
                // Файла нет локально - нужно скачать
                filesToUpdate.Add(serverFile);
            }
            else if (localFile.Size == serverFile.Size)
            {
                // Размеры совпадают - проверяем хеш
                using var algorithm = SHA1.Create();
                var localPath = Path.Combine(rootDirectory, serverFile.Directory.TrimStart('\\'));
                if (!localPath.StartsWith(Path.Combine(rootDirectory, "assets")) &&
                    SystemHelper.CalculateFileHash(localPath, algorithm) != serverFile.Hash)
                {
                    filesToUpdate.Add(serverFile);
                    filesToDelete.TryRemove(NormalizePath(localFile.Directory), out _);
                }
                else
                {
                    // Файл актуален - исключаем из списка на удаление
                    filesToDelete.TryRemove(NormalizePath(localFile.Directory), out _);
                }
            }
            else
            {
                // Размеры не совпадают - нужно обновить
                filesToUpdate.Add(serverFile);
                filesToDelete.TryRemove(NormalizePath(localFile.Directory), out _);
            }
        }));

        result.FilesToUpdate = filesToUpdate;
        result.FilesToDelete = filesToDelete.Values;
        return result;
    }

    private string NormalizePath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }
}
