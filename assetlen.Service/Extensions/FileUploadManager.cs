using assetlen.ServiceHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Service.DbServices.ServiceInterfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace assetlen.Service.Extensions
{
    public class FileUploadManager
    {
        private readonly IWebHostEnvironment _webHost;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IExcelDomainService _excelDomainService;
        private readonly ILogger<FileUploadManager> _logger;


        public FileUploadManager(IWebHostEnvironment webHost, IHttpContextAccessor contextAccessor, IExcelDomainService excelDomainService, ILogger<FileUploadManager> logger)
        {
            _webHost = webHost;
            _contextAccessor = contextAccessor;
            _excelDomainService = excelDomainService;
            _logger = logger;
        }

        #region Image Upload helper function
        public async Task<ServiceResult<ImageInfo>> HandleImageUploadAsync(IFormFile file, string saveLocation)
        {
            if (file == null) return ServiceResult<ImageInfo>.Failure(new
                BadRequestException("No file was uploaded."));

            long maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxFileSize)
                return ServiceResult<ImageInfo>.Failure(
                    new BadRequestException("File size must not exceed 5MB."));

            var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!validImageTypes.Contains(file.ContentType))
                return ServiceResult<ImageInfo>.Failure(new BadRequestException("Invalid image format."));

            try
            {
                //string UploadDir = Path.Combine(_webHost.WebRootPath, saveLocation);
                string UploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", saveLocation);

                if (!Directory.Exists(UploadDir))
                {
                    Directory.CreateDirectory(UploadDir);
                }

                string fileName = Path.GetFileName(file.FileName);
                Guid imageGuid = Guid.NewGuid();

                string fileUniqueName = imageGuid.ToString() + "_" + fileName;
                string filePath = Path.Combine(UploadDir, fileUniqueName);
                _logger.LogInformation("File Path: {FilePath}", filePath);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                string baseUrl = $"{_contextAccessor.HttpContext?.Request.Scheme}://{_contextAccessor.HttpContext?.Request.Host}/{saveLocation}";
                string accessPath = $"{baseUrl}/{fileUniqueName}";

                var imageInfo = new ImageInfo
                {
                    ImageName = fileName,
                    ImageGuid = imageGuid,
                    ImageUniqueName = fileUniqueName,
                    ImageFullPath = accessPath,
                };

                return ServiceResult<ImageInfo>.Success(imageInfo);
            }
            catch (Exception ex)
            {
                return ServiceResult<ImageInfo>.Failure(new Exception(ex.Message));
            }
        }
        #endregion

        #region Byte Array Upload helper function
        public async Task<ServiceResult<ImageInfo>> HandleByteArrayUploadAsync(
            byte[] fileBytes,
            string originalFileName,
            string contentType,
            string saveLocation)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                return ServiceResult<ImageInfo>.Failure(new BadRequestException("No file data received."));

            long maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (fileBytes.Length > maxFileSize)
                return ServiceResult<ImageInfo>.Failure(
                    new BadRequestException("File size must not exceed 5MB."));

            var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!validImageTypes.Contains(contentType))
                return ServiceResult<ImageInfo>.Failure(new BadRequestException("Invalid image format."));

            try
            {
                string uploadDir = Path.Combine(_webHost.WebRootPath, saveLocation);

                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                Guid imageGuid = Guid.NewGuid();
                string fileExtension = Path.GetExtension(originalFileName);
                string fileUniqueName = $"{imageGuid}_{originalFileName}";
                string filePath = Path.Combine(uploadDir, fileUniqueName);

                await File.WriteAllBytesAsync(filePath, fileBytes);

                string baseUrl = $"{_contextAccessor.HttpContext?.Request.Scheme}://{_contextAccessor.HttpContext?.Request.Host}/{saveLocation}";
                string accessPath = $"{baseUrl}/{fileUniqueName}";

                var imageInfo = new ImageInfo
                {
                    ImageName = originalFileName,
                    ImageGuid = imageGuid,
                    ImageUniqueName = fileUniqueName,
                    ImageFullPath = accessPath,
                    ContentType = contentType
                };

                return ServiceResult<ImageInfo>.Success(imageInfo);
            }
            catch (Exception ex)
            {
                return ServiceResult<ImageInfo>.Failure(new Exception($"Error processing file: {ex.Message}"));
            }
        }
        #endregion

        #region Excel Upload helper function
        public async Task<ServiceResult<List<Dictionary<string, object>>>> HandleExcelUploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return ServiceResult<List<Dictionary<string, object>>>.Failure(new
                BadRequestException("No file was uploaded."));

            long maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxFileSize)
                return ServiceResult<List<Dictionary<string, object>>>.Failure(
                    new BadRequestException("File size must not exceed 10MB."));

            var validImageTypes = new[] { "xlsx", "xls", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };
            if (!validImageTypes.Contains(file.ContentType))
                return ServiceResult<List<Dictionary<string, object>>>.Failure(new BadRequestException("Invalid Excel format."));

            try
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset stream position for reading

                var output = await _excelDomainService.ImportExcelRecords(memoryStream);

                return ServiceResult<List<Dictionary<string, object>>>.Success(output);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while processing excel file for import : {ex}", ex);
                return ServiceResult<List<Dictionary<string, object>>>.Failure(new BadRequestException($"Error while processing excel file for import : {ex.Message}"));
            }
        }
        #endregion
    }
}
