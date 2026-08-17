#!/usr/bin/env node
/**
 * ASSETLEN — icon generator
 * ─────────────────────────────────────────────────────────────────────────────
 * Rasterises the ASSETLEN mark to every size the browser, the OS and the PWA
 * manifest ask for, from the *same geometry* the in-app <BrandMark> component
 * draws. That is the whole point of this file existing: a favicon exported by
 * hand drifts from the component the moment either is touched, and a product
 * whose tab icon is not quite its logo reads as unfinished.
 *
 * The mark: a set square standing on a datum — two strokes rising to an apex,
 * a level line struck through them, and one terracotta node at the apex, the
 * point where a commitment is fixed.
 *
 * No image libraries. Node's zlib writes the PNGs and the ICO wraps PNGs
 * directly (allowed since Vista), so this runs on a clean checkout with nothing
 * installed.
 *
 * Usage:  node tools/make-icons.mjs
 */

import { deflateSync } from "node:zlib";
import { writeFileSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const OUT = join(dirname(fileURLToPath(import.meta.url)), "..", "assetlen.Client", "wwwroot");

// ── Palette ────────────────────────────────────────────────────────────────
// Ink ground with paper strokes: the mark has to hold up on a browser tab
// against an unknown background, and the dark ground is what makes it legible
// in both themes without shipping two files.
const INK = [0x16, 0x19, 0x1c];
const PAPER = [0xf6, 0xf5, 0xf1];
const TERRACOTTA = [0xdd, 0x7a, 0x58];

// ── Geometry, in the same 24-unit box as BrandMark.razor ───────────────────
const APEX = [12, 4.2];
const FOOT_L = [3.2, 20.2];
const FOOT_R = [20.8, 20.2];
const DATUM_L = [6.9, 14.6];
const DATUM_R = [17.1, 14.6];
const STROKE = 2.0;
const DATUM_STROKE = 1.9;
const NODE_R = 2.1;

const SS = 4; // supersampling factor per axis — 16 samples per output pixel

/** Distance from point p to segment ab. */
function distToSegment(px, py, [ax, ay], [bx, by]) {
  const dx = bx - ax;
  const dy = by - ay;
  const lenSq = dx * dx + dy * dy;
  let t = lenSq === 0 ? 0 : ((px - ax) * dx + (py - ay) * dy) / lenSq;
  t = Math.max(0, Math.min(1, t));
  const cx = ax + t * dx;
  const cy = ay + t * dy;
  return Math.hypot(px - cx, py - cy);
}

function mix(under, over, alpha) {
  return [
    Math.round(under[0] + (over[0] - under[0]) * alpha),
    Math.round(under[1] + (over[1] - under[1]) * alpha),
    Math.round(under[2] + (over[2] - under[2]) * alpha),
  ];
}

/**
 * Colour at one sample, in mark-space units.
 * Returns null outside the rounded ground (transparent corners).
 */
function sample(ux, uy, opts) {
  const { size, inset, radius, fullBleed } = opts;

  // Rounded-rectangle ground in *pixel* space
  if (!fullBleed) {
    const px = ux;
    const py = uy;
    const r = radius;
    const cx = Math.min(Math.max(px, r), size - r);
    const cy = Math.min(Math.max(py, r), size - r);
    if (Math.hypot(px - cx, py - cy) > r) return null;
  }

  // Convert pixel space → the 24-unit mark box
  const scale = (size - inset * 2) / 24;
  const mx = (ux - inset) / scale;
  const my = (uy - inset) / scale;

  let colour = INK;

  // Datum first, so the apex strokes and the node paint over it.
  if (opts.withDatum) {
    const d = distToSegment(mx, my, DATUM_L, DATUM_R);
    if (d <= DATUM_STROKE / 2) colour = mix(colour, PAPER, 0.55);
  }

  // The two rising strokes.
  const dl = distToSegment(mx, my, FOOT_L, APEX);
  const dr = distToSegment(mx, my, APEX, FOOT_R);
  if (Math.min(dl, dr) <= STROKE / 2) colour = PAPER;

  // The node — always terracotta, in every theme. A logo that changes colour
  // with the theme stops being a logo.
  if (Math.hypot(mx - APEX[0], my - APEX[1]) <= NODE_R) colour = TERRACOTTA;

  return colour;
}

/** Renders RGBA pixels at `size`, supersampled. */
function render(size, opts) {
  const o = {
    size,
    inset: opts.inset ?? size * 0.17,
    radius: opts.radius ?? size * 0.22,
    fullBleed: opts.fullBleed ?? false,
    withDatum: opts.withDatum ?? true,
  };

  const px = Buffer.alloc(size * size * 4);

  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      let r = 0, g = 0, b = 0, a = 0;

      for (let sy = 0; sy < SS; sy++) {
        for (let sx = 0; sx < SS; sx++) {
          const c = sample(x + (sx + 0.5) / SS, y + (sy + 0.5) / SS, o);
          if (c) { r += c[0]; g += c[1]; b += c[2]; a += 255; }
        }
      }

      const n = SS * SS;
      const i = (y * size + x) * 4;

      if (a === 0) {
        px[i] = px[i + 1] = px[i + 2] = px[i + 3] = 0;
      } else {
        // Average only over covered samples so edges do not darken toward black.
        const covered = a / 255;
        px[i] = Math.round(r / covered);
        px[i + 1] = Math.round(g / covered);
        px[i + 2] = Math.round(b / covered);
        px[i + 3] = Math.round(a / n);
      }
    }
  }

  return px;
}

// ── PNG encoding ───────────────────────────────────────────────────────────

const CRC_TABLE = (() => {
  const t = new Int32Array(256);
  for (let n = 0; n < 256; n++) {
    let c = n;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    t[n] = c;
  }
  return t;
})();

function crc32(buf) {
  let c = -1;
  for (let i = 0; i < buf.length; i++) c = CRC_TABLE[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return (c ^ -1) >>> 0;
}

function chunk(type, data) {
  const len = Buffer.alloc(4);
  len.writeUInt32BE(data.length);
  const body = Buffer.concat([Buffer.from(type, "ascii"), data]);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(body));
  return Buffer.concat([len, body, crc]);
}

function encodePng(size, rgba) {
  // Filter byte 0 (none) per scanline. The images are tiny and flat; a smarter
  // filter would save bytes nobody is counting.
  const raw = Buffer.alloc(size * (size * 4 + 1));
  for (let y = 0; y < size; y++) {
    const dst = y * (size * 4 + 1);
    raw[dst] = 0;
    rgba.copy(raw, dst + 1, y * size * 4, (y + 1) * size * 4);
  }

  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(size, 0);
  ihdr.writeUInt32BE(size, 4);
  ihdr[8] = 8;  // bit depth
  ihdr[9] = 6;  // colour type: RGBA
  ihdr[10] = 0; // deflate
  ihdr[11] = 0; // adaptive filtering
  ihdr[12] = 0; // no interlace

  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk("IHDR", ihdr),
    chunk("IDAT", deflateSync(raw, { level: 9 })),
    chunk("IEND", Buffer.alloc(0)),
  ]);
}

/** ICO wrapping PNG entries — supported from Vista onward, and far smaller than BMP. */
function encodeIco(entries) {
  const header = Buffer.alloc(6);
  header.writeUInt16LE(0, 0);
  header.writeUInt16LE(1, 2);              // 1 = icon
  header.writeUInt16LE(entries.length, 4);

  const dir = Buffer.alloc(16 * entries.length);
  let offset = header.length + dir.length;

  entries.forEach((e, i) => {
    const at = i * 16;
    dir[at] = e.size >= 256 ? 0 : e.size;      // 0 means 256
    dir[at + 1] = e.size >= 256 ? 0 : e.size;
    dir[at + 2] = 0;                            // palette
    dir[at + 3] = 0;                            // reserved
    dir.writeUInt16LE(1, at + 4);               // colour planes
    dir.writeUInt16LE(32, at + 6);              // bits per pixel
    dir.writeUInt32LE(e.png.length, at + 8);
    dir.writeUInt32LE(offset, at + 12);
    offset += e.png.length;
  });

  return Buffer.concat([header, dir, ...entries.map((e) => e.png)]);
}

// ── Outputs ────────────────────────────────────────────────────────────────

const jobs = [
  // Browser tab. At 16px the datum line and the node collide into a smudge,
  // so the small master drops the datum and keeps the silhouette readable.
  { file: "favicon-16.png", size: 16, opts: { withDatum: false, inset: 1.5, radius: 3 } },
  { file: "favicon-32.png", size: 32, opts: { inset: 4, radius: 7 } },
  { file: "favicon.png", size: 48, opts: { inset: 6, radius: 10 } },

  // iOS home screen: no transparency, no rounded corners of our own — the OS
  // masks it, and a pre-rounded icon gets rounded twice.
  { file: "apple-touch-icon.png", size: 180, opts: { fullBleed: true, inset: 34 } },

  { file: "icon-192.png", size: 192, opts: { inset: 33, radius: 42 } },
  { file: "icon-512.png", size: 512, opts: { inset: 88, radius: 112 } },

  // Maskable: Android crops to a circle inscribed in the safe zone, so the mark
  // sits inside 40% of the radius from centre and the ground goes edge to edge.
  { file: "icon-maskable-512.png", size: 512, opts: { fullBleed: true, inset: 138 } },
];

for (const job of jobs) {
  const png = encodePng(job.size, render(job.size, job.opts));
  writeFileSync(join(OUT, job.file), png);
  console.log(`  ${job.file.padEnd(26)} ${job.size}×${job.size}  ${png.length} bytes`);
}

const ico = encodeIco([16, 32, 48].map((size) => ({
  size,
  png: encodePng(size, render(size, {
    withDatum: size > 16,
    inset: size * 0.11,
    radius: size * 0.2,
  })),
})));

writeFileSync(join(OUT, "favicon.ico"), ico);
console.log(`  favicon.ico                16/32/48  ${ico.length} bytes`);

// The SVG is authored, not generated — but it must stay in step with the
// geometry above, so it is written from the same constants.
const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" role="img" aria-label="ASSETLEN">
  <title>ASSETLEN</title>
  <rect width="24" height="24" rx="5.2" fill="#16191c"/>
  <g transform="translate(12 12) scale(0.78) translate(-12 -12)">
    <path d="M${FOOT_L[0]} ${FOOT_L[1]} L${APEX[0]} ${APEX[1]} L${FOOT_R[0]} ${FOOT_R[1]}"
          fill="none" stroke="#f6f5f1" stroke-width="${STROKE}"
          stroke-linecap="round" stroke-linejoin="round"/>
    <line x1="${DATUM_L[0]}" y1="${DATUM_L[1]}" x2="${DATUM_R[0]}" y2="${DATUM_R[1]}"
          stroke="#f6f5f1" stroke-width="${DATUM_STROKE}" stroke-linecap="round" opacity="0.55"/>
    <circle cx="${APEX[0]}" cy="${APEX[1]}" r="${NODE_R}" fill="#dd7a58"/>
  </g>
</svg>
`;

writeFileSync(join(OUT, "favicon.svg"), svg);
console.log(`  favicon.svg                vector    ${Buffer.byteLength(svg)} bytes`);
