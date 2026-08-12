// Hand the browser bytes that were fetched by .NET.
//
// Artifact content sits behind an authenticated endpoint, so it cannot be
// reached with an <a href> or an <img src> — the browser sends neither the
// bearer token nor the API origin. The caller fetches through the Refit
// pipeline and passes the bytes here.
//
// Loaded as an ES module via IJSRuntime "import", not from index.html: this
// file ships inside the Razor Class Library, and the app has two scripts.js
// files of which only the client's is actually referenced.

/**
 * Save bytes to the user's downloads.
 * @param {string} fileName  suggested name
 * @param {string} mimeType  content type
 * @param {Uint8Array} bytes the content
 */
export function save(fileName, mimeType, bytes) {
    const url = toObjectUrl(mimeType, bytes);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName || "download";
    document.body.appendChild(link);
    link.click();
    link.remove();

    // Revoking immediately can cancel the download in Safari, so give the
    // navigation a moment to start. The blob is small-lived either way.
    setTimeout(() => URL.revokeObjectURL(url), 10_000);
}

/**
 * Open bytes in a new tab — for PDFs and images, where a preview beats a file
 * on disk. Returns false when the popup was blocked so the caller can fall
 * back to save().
 */
export function open(mimeType, bytes) {
    const url = toObjectUrl(mimeType, bytes);
    const win = window.open(url, "_blank", "noopener");
    if (!win) {
        URL.revokeObjectURL(url);
        return false;
    }
    setTimeout(() => URL.revokeObjectURL(url), 60_000);
    return true;
}

function toObjectUrl(mimeType, bytes) {
    return URL.createObjectURL(
        new Blob([bytes], { type: mimeType || "application/octet-stream" }));
}
