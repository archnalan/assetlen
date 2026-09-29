// Voice notes, recorded the way WhatsApp records them: press, talk, stop.
let recorder = null;
let chunks = [];
let stream = null;

export function supported() {
    return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.MediaRecorder);
}

export async function start() {
    stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    const type = ['audio/webm;codecs=opus', 'audio/ogg;codecs=opus', 'audio/mp4'].find(t => MediaRecorder.isTypeSupported(t)) || '';
    recorder = new MediaRecorder(stream, type ? { mimeType: type } : undefined);
    chunks = [];
    recorder.ondataavailable = e => { if (e.data && e.data.size) chunks.push(e.data); };
    recorder.start();
}

export function stop() {
    return new Promise(resolve => {
        if (!recorder) { resolve(null); return; }
        recorder.onstop = async () => {
            const type = (recorder.mimeType || 'audio/webm').split(';')[0];
            const blob = new Blob(chunks, { type });
            stream.getTracks().forEach(t => t.stop());
            recorder = null; stream = null;
            resolve({ type, data: new Uint8Array(await blob.arrayBuffer()), url: URL.createObjectURL(blob) });
        };
        recorder.stop();
    });
}

export function cancel() {
    try { if (recorder) recorder.stop(); } catch { }
    if (stream) stream.getTracks().forEach(t => t.stop());
    recorder = null; stream = null; chunks = [];
}
