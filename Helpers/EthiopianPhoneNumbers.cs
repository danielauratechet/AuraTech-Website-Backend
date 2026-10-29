namespace AuratechBackend.Helpers;

public static class EthiopianPhoneNumbers
{
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var compact = new string(input.Where(c => !char.IsWhiteSpace(c) && c is not ('-' or '(' or ')')).ToArray());
        if (compact.Length == 0)
        {
            return false;
        }

        string subscriber;
        if (compact.StartsWith("+251", StringComparison.Ordinal))
        {
            subscriber = compact[4..];
        }
        else if (compact.StartsWith("251", StringComparison.Ordinal))
        {
            subscriber = compact[3..];
        }
        else if (compact[0] == '0')
        {
            subscriber = compact[1..];
        }
        else
        {
            subscriber = compact;
        }

        if (subscriber.Length != 9 || subscriber[0] is not ('7' or '9') || !subscriber.All(char.IsDigit))
        {
            return false;
        }

        normalized = "+251" + subscriber;
        return true;
    }
}
