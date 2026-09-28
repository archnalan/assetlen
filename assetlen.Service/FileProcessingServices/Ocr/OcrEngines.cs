using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace assetlen.Service.FileProcessingServices.Ocr;

public sealed record OcrResult(bool Success, string? Text, string? Error);

/// <summary>Reads text out of an image on disk.</summary>
public interface IImageOcrEngine
{
    string Name { get; }
    bool IsAvailable { get; }
    Task<OcrResult> ReadAsync(string filePath, CancellationToken ct = default);
}

/// <summary>
/// Chooses how to read an artifact, by type. Text files are read as they are;
/// images go to whichever OCR engine this server has; anything else is reported
/// as unsupported rather than silently recorded as empty.
/// </summary>
public interface IOcrService
{
    /// <summary>The image engine in use, or null when none is configured.</summary>
    string? ImageEngine { get; }

    bool CanRead(string? mimeType);

    bool IsImage(string? mimeType);

    Task<OcrResult> ReadAsync(string filePath, string? mimeType, CancellationToken ct = default);
}

public sealed class OcrService : IOcrService
{
    private readonly IImageOcrEngine? _image;

    public OcrService(IEnumerable<IImageOcrEngine> engines, IConfiguration config)
    {
        var wanted = config["Ocr:Engine"]?.Trim().ToLowerInvariant() ?? "auto";
        var available = engines.Where(e => e.IsAvailable).ToList();

        _image = wanted switch
        {
            "none" => null,
            "auto" => available.FirstOrDefault(),
            _ => available.FirstOrDefault(e => e.Name.StartsWith(wanted, StringComparison.OrdinalIgnoreCase))
        };
    }

    public string? ImageEngine => _image?.Name;

    public bool IsImage(string? mimeType) =>
        mimeType is "image/jpeg" or "image/png" or "image/bmp" or "image/gif" or "image/tiff" or "image/webp";

    private static bool IsText(string? mimeType) =>
        mimeType is not null && (mimeType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                                 || mimeType is "application/json" or "application/xml");

    public bool CanRead(string? mimeType) => IsText(mimeType) || (IsImage(mimeType) && _image is not null);

    public async Task<OcrResult> ReadAsync(string filePath, string? mimeType, CancellationToken ct = default)
    {
        if (IsText(mimeType))
            return new OcrResult(true, await File.ReadAllTextAsync(filePath, Encoding.UTF8, ct), null);

        if (IsImage(mimeType) && _image is not null)
            return await _image.ReadAsync(filePath, ct);

        return new OcrResult(false, null, "No engine for this type.");
    }
}

/// <summary>
/// Windows' own OCR (<c>Windows.Media.Ocr</c>), reached through Windows PowerShell
/// because the API host targets plain <c>net10.0</c>. No install, no model files —
/// it is what makes OCR run on the development machine out of the box. A Linux
/// deployment uses <see cref="TesseractOcrEngine"/> instead.
/// </summary>
public sealed class WindowsOcrEngine : IImageOcrEngine
{
    private readonly ILogger<WindowsOcrEngine> _logger;
    private static readonly SemaphoreSlim ScriptLock = new(1, 1);
    private static string? _scriptPath;

    public WindowsOcrEngine(ILogger<WindowsOcrEngine> logger) => _logger = logger;

    public string Name => "windows-ocr";

    private static string PowerShellPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");

    public bool IsAvailable => OperatingSystem.IsWindows() && File.Exists(PowerShellPath);

    public async Task<OcrResult> ReadAsync(string filePath, CancellationToken ct = default)
    {
        var script = await EnsureScriptAsync(ct);

        var psi = new ProcessStartInfo(PowerShellPath)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };
        foreach (var a in new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", script, "-Path", filePath })
            psi.ArgumentList.Add(a);

        using var process = Process.Start(psi);
        if (process is null) return new OcrResult(false, null, "Could not start PowerShell.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(90));

        var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
            return new OcrResult(false, null, "OCR timed out.");
        }

        var text = (await stdout).Trim();
        var error = (await stderr).Trim();

        if (process.ExitCode != 0)
        {
            _logger.LogWarning("Windows OCR failed for {File}: {Error}", filePath, error);
            return new OcrResult(false, null, string.IsNullOrEmpty(error) ? $"Exit code {process.ExitCode}." : Truncate(error, 900));
        }

        return new OcrResult(true, text, null);
    }

    private static async Task<string> EnsureScriptAsync(CancellationToken ct)
    {
        if (_scriptPath is not null && File.Exists(_scriptPath)) return _scriptPath;

        await ScriptLock.WaitAsync(ct);
        try
        {
            if (_scriptPath is not null && File.Exists(_scriptPath)) return _scriptPath;
            var path = Path.Combine(Path.GetTempPath(), "assetlen-ocr-v1.ps1");
            await File.WriteAllTextAsync(path, Script, new UTF8Encoding(true), ct);
            _scriptPath = path;
            return path;
        }
        finally
        {
            ScriptLock.Release();
        }
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    private const string Script = """
        param([string]$Path)
        $ErrorActionPreference = 'Stop'
        [Console]::OutputEncoding = [Text.Encoding]::UTF8
        Add-Type -AssemblyName System.Runtime.WindowsRuntime
        $null = [Windows.Storage.StorageFile, Windows.Storage, ContentType = WindowsRuntime]
        $null = [Windows.Media.Ocr.OcrEngine, Windows.Foundation, ContentType = WindowsRuntime]
        $null = [Windows.Graphics.Imaging.BitmapDecoder, Windows.Graphics, ContentType = WindowsRuntime]
        $asTask = ([System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' })[0]
        function Await($op, [Type]$t) { $task = $asTask.MakeGenericMethod($t).Invoke($null, @($op)); $task.Wait(); $task.Result }
        $engine = [Windows.Media.Ocr.OcrEngine]::TryCreateFromUserProfileLanguages()
        if ($null -eq $engine) { [Console]::Error.WriteLine('No OCR language is installed for this account.'); exit 2 }
        $file = Await ([Windows.Storage.StorageFile]::GetFileFromPathAsync($Path)) ([Windows.Storage.StorageFile])
        $stream = Await ($file.OpenAsync([Windows.Storage.FileAccessMode]::Read)) ([Windows.Storage.Streams.IRandomAccessStream])
        $decoder = Await ([Windows.Graphics.Imaging.BitmapDecoder]::CreateAsync($stream)) ([Windows.Graphics.Imaging.BitmapDecoder])
        $max = [Windows.Media.Ocr.OcrEngine]::MaxImageDimension
        if ($decoder.PixelWidth -gt $max -or $decoder.PixelHeight -gt $max) {
            $scale = [Math]::Min($max / $decoder.PixelWidth, $max / $decoder.PixelHeight)
            $transform = New-Object Windows.Graphics.Imaging.BitmapTransform
            $transform.ScaledWidth = [uint32]($decoder.PixelWidth * $scale)
            $transform.ScaledHeight = [uint32]($decoder.PixelHeight * $scale)
            $bitmap = Await ($decoder.GetSoftwareBitmapAsync([Windows.Graphics.Imaging.BitmapPixelFormat]::Bgra8, [Windows.Graphics.Imaging.BitmapAlphaMode]::Premultiplied, $transform, [Windows.Graphics.Imaging.ExifOrientationMode]::RespectExifOrientation, [Windows.Graphics.Imaging.ColorManagementMode]::DoNotColorManage)) ([Windows.Graphics.Imaging.SoftwareBitmap])
        } else {
            $bitmap = Await ($decoder.GetSoftwareBitmapAsync()) ([Windows.Graphics.Imaging.SoftwareBitmap])
        }
        $result = Await ($engine.RecognizeAsync($bitmap)) ([Windows.Media.Ocr.OcrResult])
        $result.Lines | ForEach-Object { $_.Text }
        """;
}

/// <summary>
/// Tesseract, when the server has it: set <c>Ocr:TesseractPath</c>, or have
/// <c>tesseract</c> on the PATH. The portable choice for a Linux host.
/// </summary>
public sealed class TesseractOcrEngine : IImageOcrEngine
{
    private readonly string? _path;

    public TesseractOcrEngine(IConfiguration config)
    {
        var configured = config["Ocr:TesseractPath"];
        _path = !string.IsNullOrWhiteSpace(configured) && File.Exists(configured) ? configured : FindOnPath();
    }

    public string Name => "tesseract";

    public bool IsAvailable => _path is not null;

    public async Task<OcrResult> ReadAsync(string filePath, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo(_path!)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };
        psi.ArgumentList.Add(filePath);
        psi.ArgumentList.Add("stdout");

        using var process = Process.Start(psi);
        if (process is null) return new OcrResult(false, null, "Could not start tesseract.");

        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        return process.ExitCode == 0
            ? new OcrResult(true, stdout.Trim(), null)
            : new OcrResult(false, null, stderr.Length > 900 ? stderr[..900] : stderr);
    }

    private static string? FindOnPath()
    {
        var exe = OperatingSystem.IsWindows() ? "tesseract.exe" : "tesseract";
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
