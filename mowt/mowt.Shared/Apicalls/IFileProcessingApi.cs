using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Refit;

namespace mowt.Shared.Apicalls
{
    public interface IFileProcessingApi
    {
        [Multipart]
        [Post("/api/FileProcessing/ProcessExcelFile")]
        Task<IApiResponse<List<Dictionary<string, object>>>> ProcessExcelFile(StreamPart ImportFile, string ImportName);
    }
}
