namespace assetlen.Service.DbServices.ServiceInterfaces;

/// <summary>
/// Where artifact bytes physically live. Content-addressed: the caller supplies
/// the hash, the store decides the path. Swapping local disk for Blob or Drive
/// is an implementation change here and nothing else — <c>tbl_Artifact</c>
/// stores a <em>relative</em> path for exactly this reason.
/// </summary>
public interface IArtifactStorage
{
    /// <summary>
    /// Write bytes at the content address for <paramref name="sha256"/>.
    /// Idempotent: if the address already holds bytes, this is a no-op and the
    /// existing path is returned. Returns the relative storage path.
    /// </summary>
    Task<string> PutAsync(string sha256, string extension, Stream content, CancellationToken ct = default);

    /// <summary>Write a derived thumbnail beside the original. Returns its relative path.</summary>
    Task<string> PutThumbnailAsync(string sha256, string extension, Stream content, CancellationToken ct = default);

    /// <summary>Open a stored artifact for reading. Null when the path holds nothing.</summary>
    Task<Stream?> OpenAsync(string relativePath, CancellationToken ct = default);

    /// <summary>True when bytes already exist at this content address.</summary>
    Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default);
}
