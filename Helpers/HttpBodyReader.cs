using System.Text;

namespace AuratechBackend.Helpers;

public static class HttpBodyReader
{
    public static async Task<string> ReadAsStringAsync(Stream stream, int maxLength, CancellationToken cancellationToken = default)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var buffer = new char[Math.Min(maxLength + 1, 4096)];
        var builder = new StringBuilder();

        while (builder.Length <= maxLength)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
            {
                break;
            }

            var remaining = maxLength - builder.Length;
            if (read > remaining)
            {
                builder.Append(buffer, 0, remaining);
                builder.Append("...[truncated]");
                break;
            }

            builder.Append(buffer, 0, read);
        }

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return builder.ToString();
    }
}
