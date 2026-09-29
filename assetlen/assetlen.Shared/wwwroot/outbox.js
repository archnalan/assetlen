// The capture outbox. A site at 22:00 has one bar or none; a capture is written
// here first and sent when there is signal, so posting never waits on the network
// and nothing shot is lost to a dropped connection (assetlen.md §9).
const DB = 'assetlen-outbox';
const STORE = 'captures';

function open() {
    return new Promise((resolve, reject) => {
        const req = indexedDB.open(DB, 1);
        req.onupgradeneeded = () => req.result.createObjectStore(STORE, { keyPath: 'id' });
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
    });
}

async function tx(mode, fn) {
    const db = await open();
    return new Promise((resolve, reject) => {
        const t = db.transaction(STORE, mode);
        const store = t.objectStore(STORE);
        let result;
        Promise.resolve(fn(store, r => { result = r; })).catch(reject);
        t.oncomplete = () => { db.close(); resolve(result); };
        t.onerror = () => { db.close(); reject(t.error); };
    });
}

function read(store, id) {
    return new Promise((resolve, reject) => {
        const r = store.get(id);
        r.onsuccess = () => resolve(r.result);
        r.onerror = () => reject(r.error);
    });
}

export function available() {
    try { return typeof indexedDB !== 'undefined' && indexedDB !== null; } catch { return false; }
}

export function isOnline() { return navigator.onLine; }

export async function begin(meta) {
    await tx('readwrite', store => store.put({ ...meta, files: [], voice: null, ready: false, attempts: 0, lastError: null }));
}

export async function addFile(id, name, type, caption, bytes) {
    await tx('readwrite', async store => {
        const item = await read(store, id);
        item.files.push({ name, type, caption, data: new Blob([bytes], { type }) });
        store.put(item);
    });
}

export async function setVoice(id, name, type, bytes) {
    await tx('readwrite', async store => {
        const item = await read(store, id);
        item.voice = { name, type, data: new Blob([bytes], { type }) };
        store.put(item);
    });
}

export async function commit(id) {
    await tx('readwrite', async store => {
        const item = await read(store, id);
        item.ready = true;
        store.put(item);
    });
}

// Metadata only; bytes are read one file at a time when sending.
export async function list() {
    const items = await tx('readonly', (store, done) => new Promise((resolve, reject) => {
        const r = store.getAll();
        r.onsuccess = () => { done(r.result); resolve(); };
        r.onerror = () => reject(r.error);
    }));
    return (items || []).map(i => ({
        id: i.id, projectId: i.projectId, deliverableId: i.deliverableId, stageId: i.stageId,
        label: i.label, description: i.description, completionPercentage: i.completionPercentage,
        hasIssues: !!i.hasIssues, channel: i.channel, capturedAt: i.capturedAt,
        frameCount: i.files.length, hasVoice: !!i.voice, ready: !!i.ready,
        attempts: i.attempts || 0, lastError: i.lastError, captions: i.files.map(f => f.caption || ''),
        names: i.files.map(f => f.name), types: i.files.map(f => f.type),
        voiceName: i.voice ? i.voice.name : null, voiceType: i.voice ? i.voice.type : null
    }));
}

export async function fileBytes(id, index) {
    const item = await tx('readonly', async (store, done) => done(await read(store, id)));
    const f = item && item.files[index];
    return f ? new Uint8Array(await f.data.arrayBuffer()) : null;
}

export async function voiceBytes(id) {
    const item = await tx('readonly', async (store, done) => done(await read(store, id)));
    return item && item.voice ? new Uint8Array(await item.voice.data.arrayBuffer()) : null;
}

export async function markFailed(id, error) {
    await tx('readwrite', async store => {
        const item = await read(store, id);
        if (!item) return;
        item.attempts = (item.attempts || 0) + 1;
        item.lastError = error;
        store.put(item);
    });
}

export async function remove(id) {
    await tx('readwrite', store => store.delete(id));
}

// Wakes the outbox when the signal comes back, and when the tab returns to the foreground.
export function watch(dotnet) {
    const kick = () => dotnet.invokeMethodAsync('Kick').catch(() => { });
    window.addEventListener('online', kick);
    document.addEventListener('visibilitychange', () => { if (!document.hidden) kick(); });
}
