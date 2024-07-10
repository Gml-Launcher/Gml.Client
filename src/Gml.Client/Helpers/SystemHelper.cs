using System.Security.Cryptography;

namespace Gml.Client.Helpers;

public class SystemHelper
{
    public static string CalculateFileHash(string filePath, HashAlgorithm algorithm)
    {
        using (var fileStream = new BufferedStream(File.OpenRead(filePath), 1200000))
        {
            var hashBytes = algorithm.ComputeHash(fileStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
