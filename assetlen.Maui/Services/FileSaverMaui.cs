using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Services;

namespace assetlen.Maui.Services;

/// <summary>
/// Saving an artifact to disk is a native operation here rather than a browser
/// download, so a Site Diary export lands where the reader put it and keeps its
/// name — the permanent address the charter asks an Artifact to have (§1).
/// </summary>
public class FileSaverMaui : ICustomFileSaver
{
    public async Task<ServiceResult<string>> SaveFileAsync(
        string defaultFileName,
        string fileExtension,
        MemoryStream stream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var name = Path.HasExtension(defaultFileName)
                ? defaultFileName
                : $"{defaultFileName}.{fileExtension.TrimStart('.')}";

            var path = Path.Combine(FileSystem.AppDataDirectory, name);

            await using (var file = File.Create(path))
            {
                stream.Position = 0;
                await stream.CopyToAsync(file, cancellationToken);
            }

            return ServiceResult<string>.Success(path);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Failure(ex);
        }
    }

    public async Task<ServiceResult<bool>> OpenFileWithDefaultAppAsync(string fullPath)
    {
        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(fullPath)
            });
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ex);
        }
    }

    public async Task<FileResultDto> PickFileFromSystem()
    {
        var picked = await FilePicker.Default.PickAsync();
        if (picked is null) return new FileResultDto();

        return new FileResultDto
        {
            FileName = picked.FileName,
            ContentType = picked.ContentType,
            FullPath = picked.FullPath
        };
    }
}

public class PrintServiceMaui : IPrintService
{
    public Task<ServiceResult<List<string>>> GetPrintersWindowsAsync() =>
        Task.FromResult(ServiceResult<List<string>>.Success(new List<string>()));

    public Task<ServiceResult<bool>> PrintDocumentAsync(MemoryStream pdfDocument, string printerName = "") =>
        Task.FromResult(ServiceResult<bool>.Failure(new NotSupportedException("Printing is not wired up on the native head yet.")));

    public Task<ServiceResult<bool>> PrintImageAsync(MemoryStream imageStream, string printerName = "") =>
        Task.FromResult(ServiceResult<bool>.Failure(new NotSupportedException("Printing is not wired up on the native head yet.")));
}

public class FolderPickerService : IFolderPickerService
{
    public Task<string> PickFolderAsync() => Task.FromResult(FileSystem.AppDataDirectory);
}
