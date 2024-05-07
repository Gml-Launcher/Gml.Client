using System.Security.Cryptography;
using static System.OperatingSystem;


namespace Gml.Client.Helpers;

public class SystemHelper
{
    public static string CalculateFileHash(string filePath, HashAlgorithm algorithm)
    {
        using var fileStream = File.OpenRead(filePath);
        var hashBytes = algorithm.ComputeHash(fileStream);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
