namespace AuratechBackend.Services.Storage;

public interface ISupabaseStorageService
{
    /// <summary>
    /// Creates a signed upload URL for the given object key.
    /// </summary>
    /// <param name="objectKey">Object key path in Supabase Storage (e.g. kyc/1/tin_certificate/file.pdf)</param>
    /// <param name="fileName">Original file name (unused for signing; kept for call-site clarity)</param>
    Task<(string UploadUrl, string PublicUrl)> CreateSignedUploadUrlAsync(string objectKey, string fileName);
}
