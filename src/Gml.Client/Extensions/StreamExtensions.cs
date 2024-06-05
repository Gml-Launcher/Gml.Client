namespace Gml.Client.Extensions;

public static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, long totalBytes,
        IProgress<int> progress,
        CancellationToken cancellationToken = default, int bufferSize = 81920)
    {
        var buffer = new byte[bufferSize];
        int bytesRead;
        long totalBytesRead = 0;

        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalBytesRead += bytesRead;
            var percentage = totalBytes > 0 ? (int)((double)totalBytesRead / totalBytes * 100) : 0;
            progress?.Report(percentage);
        }
    }
}
