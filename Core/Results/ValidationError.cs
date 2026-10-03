namespace AuratechBackend.Core.Results;

public sealed record ValidationError(string PropertyName, string Message);
