using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using Microsoft.Extensions.Logging;

namespace assetlen.Service.FileProcessingServices
{
    public class FileProcessing : IFileProcessing
    {
        private readonly FileUploadManager _fileUpload;
        private readonly ILogger<FileProcessing> _logger;
        public FileProcessing(FileUploadManager fileUpload, ILogger<FileProcessing> logger)
        {
            _fileUpload = fileUpload;
            _logger = logger;
        }
        #region Process Excel File
        public async Task<ServiceResult<List<Dictionary<string, object>>>> ProcessExcelFile(ImportMedia p)
        {
            if (p == null)
                return ServiceResult<List<Dictionary<string, object>>>.Failure(
                    new BadRequestException($"File Data is required and cannot be null "));

            try
            {
                if (p.ImportFile == null)
                {
                    return ServiceResult<List<Dictionary<string, object>>>.Failure(
                        new BadRequestException($"File Data is required and cannot be null "));
                }
                var uploadedResult = await _fileUpload.HandleExcelUploadAsync(p.ImportFile);

                return uploadedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while importing media file : {ex}", ex);
                return ServiceResult<List<Dictionary<string, object>>>.Failure(new ServerErrorException("Server Error while Processing Excel file"));
            }
        }
        #endregion
    }
}
