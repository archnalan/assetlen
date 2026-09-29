using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace assetlen.Service.FileProcessingServices.Ocr;

/// <summary>
/// Turns a voice note into words. The transcript lands in <c>tbl_ArtifactText</c>
/// beside OCR text, so a spoken "forty bags" is found by the same search as a
/// photographed receipt (assetlen.md §9 parity; tier 3).
/// </summary>
public interface IAudioTranscriber
{
    string Name { get; }
    bool IsAvailable { get; }
    bool CanRead(string? mimeType);
    Task<OcrResult> TranscribeAsync(string filePath, string? mimeType, CancellationToken ct = default);
}

/// <summary>
/// Windows' own dictation recogniser (System.Speech, through Windows PowerShell),
/// fed a 16 kHz mono WAV. A browser records Opus in WebM or Ogg, so anything that
/// is not already WAV goes through ffmpeg first; with no ffmpeg only WAV is read.
/// Rough, offline and free — the deterministic fallback until a better engine is
/// configured behind the same interface.
/// </summary>
public sealed class WindowsSpeechTranscriber : IAudioTranscriber
{
    private readonly ILogger<WindowsSpeechTranscriber> _logger;
    private readonly string? _ffmpeg;
    private readonly string _culture;

    public WindowsSpeechTranscriber(IConfiguration config, ILogger<WindowsSpeechTranscriber> logger)
    {
        _logger = logger;
        _culture = config["Transcription:Culture"] ?? "en-GB";
        var configured = config["Media:FfmpegPath"];
        _ffmpeg = !string.IsNullOrWhiteSpace(configured) && File.Exists(configured) ? configured : FindOnPath("ffmpeg");
    }

    public string Name => "windows-speech";

    private static string PowerShellPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");

    public bool IsAvailable => OperatingSystem.IsWindows() && File.Exists(PowerShellPath);

    public bool CanRead(string? mimeType) =>
        mimeType is "audio/wav" or "audio/x-wav" or "audio/wave" || _ffmpeg is not null;

    public async Task<OcrResult> TranscribeAsync(string filePath, string? mimeType, CancellationToken ct = default)
    {
        var wav = filePath;
        var converted = false;
        if (mimeType is not ("audio/wav" or "audio/x-wav" or "audio/wave"))
        {
            if (_ffmpeg is null) return new OcrResult(false, null, "Only WAV can be read without ffmpeg.");
            wav = Path.Combine(Path.GetTempPath(), $"assetlen-voice-{Guid.NewGuid():N}.wav");
            var (ok, err) = await RunAsync(_ffmpeg, new[] { "-y", "-loglevel", "error", "-i", filePath, "-ac", "1", "-ar", "16000", wav }, 120, ct);
            if (!ok) return new OcrResult(false, null, $"Could not convert the recording: {err}");
            converted = true;
        }

        try
        {
            var script = Path.Combine(Path.GetTempPath(), "assetlen-transcribe-v1.ps1");
            if (!File.Exists(script)) await File.WriteAllTextAsync(script, Script, new UTF8Encoding(true), ct);
            var (ok, output) = await RunAsync(PowerShellPath,
                new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", script, "-Wav", wav, "-Culture", _culture }, 300, ct);
            return ok ? new OcrResult(true, output.Trim(), null) : new OcrResult(false, null, output);
        }
        finally
        {
            if (converted) try { File.Delete(wav); } catch { /* temp */ }
        }
    }

    private const string Script = """
        param([string]$Wav, [string]$Culture)
        $ErrorActionPreference = 'Stop'
        [Console]::OutputEncoding = [Text.Encoding]::UTF8
        Add-Type -AssemblyName System.Speech
        $r = New-Object System.Speech.Recognition.SpeechRecognitionEngine([Globalization.CultureInfo]$Culture)
        $r.LoadGrammar((New-Object System.Speech.Recognition.DictationGrammar))
        $r.SetInputToWaveFile($Wav)
        $out = @()
        while ($true) { $res = $r.Recognize(); if ($null -eq $res) { break }; $out += $res.Text }
        $r.Dispose()
        $out -join ' '
        """;

    private async Task<(bool Ok, string Output)> RunAsync(string exe, IEnumerable<string> args, int seconds, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(exe)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var process = Process.Start(psi);
        if (process is null) return (false, $"Could not start {Path.GetFileName(exe)}.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(seconds));
        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* gone */ }
            return (false, "Timed out.");
        }

        if (process.ExitCode != 0)
        {
            var error = (await stderr).Trim();
            _logger.LogWarning("{Exe} failed: {Error}", Path.GetFileName(exe), error);
            return (false, error.Length > 900 ? error[..900] : error);
        }
        return (true, await stdout);
    }

    private static string? FindOnPath(string name)
    {
        var exe = OperatingSystem.IsWindows() ? name + ".exe" : name;
        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            try
            {
                var candidate = Path.Combine(dir.Trim(), exe);
                if (File.Exists(candidate)) return candidate;
            }
            catch (ArgumentException) { /* malformed PATH entry */ }
        }
        return null;
    }
}
