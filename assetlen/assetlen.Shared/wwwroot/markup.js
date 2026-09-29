// The drawing surface over a marked-up file. Blazor gets client coordinates
// from pointer events; this turns them into fractions of the picture's box
// and keeps a drag attached to the surface when the finger leaves it.
export function box(el) {
    const r = el.getBoundingClientRect();
    return { x: r.left, y: r.top, w: r.width, h: r.height };
}

export function capture(el, pointerId) {
    try { el.setPointerCapture(pointerId); } catch { /* the pointer already ended */ }
}
