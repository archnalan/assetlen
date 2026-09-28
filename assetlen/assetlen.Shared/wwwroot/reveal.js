// Bring one element into view — where a search result opens a long list.
// A module rather than eval, loaded via IJSRuntime "import" like artifact-download.js.
export function reveal(id) {
    const el = document.getElementById(id);
    if (!el) return false;
    el.scrollIntoView({ block: "center", behavior: "smooth" });
    return true;
}
