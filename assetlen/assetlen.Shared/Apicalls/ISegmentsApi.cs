using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface ISegmentsAPI
    {
        [Get("/api/Segments/GetSegmentsFromDB")]
        Task<IApiResponse<PaginationDetails<SegmentsDto>>> GetSegmentsFromDB([Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Segments/SearchSegmentsForComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchSegmentsForComboBoxes([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Segments/SearchSegmentsUsingKeywords")]
        Task<IApiResponse<PaginationDetails<SegmentsDto>>> SearchSegmentsUsingKeywords([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Segments/GetSegmentsBasedOnSegmentId")]
        Task<IApiResponse<SegmentsDto>> GetSegmentsBasedOnSegmentId([Query] string segmentId);

        [Post("/api/Segments/AddSegment")]
        Task<IApiResponse<SegmentsDto>> AddSegment([Body] SegmentsDto SegmentsDto);

        [Put("/api/Segments/UpdateSegment")]
        Task<IApiResponse<SegmentsDto>> UpdateSegment([Body] SegmentsDto SegmentsDto);

        [Delete("/api/Segments/DeleteSegment")]
        Task<IApiResponse<SegmentsDto>> DeleteSegment([Query] string id);

        [Get("/api/Segments/SearchSegmentsFromDB")]
        Task<IApiResponse<PaginationDetails<SegmentsDto>>> SearchSegmentsFromDB([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Post("/api/Segments/ImportSegmentsFromExcel")]
        Task<IApiResponse<ImportResultSummary>> ImportSegmentsFromExcel([Body] ImportDataDto p);

        [Post("/api/Segments/GetSegmentsForCSVExportBySelectedFields")]
        Task<IApiResponse<HttpContent>> GetSegmentsForCSVExportBySelectedFields([Body] List<string> selectedColumnNames);

    }
}
