using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;

namespace mowt.Service.DbServices.ServiceInterfaces
{
	public interface ISegmentsDAL
	{
		Task<ServiceResult<SegmentsDto>> AddSegment(SegmentsDto s);
		Task<ServiceResult<bool>> DeleteSegment(string segmentId);
		Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchSegmentsForComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
		Task<ServiceResult<string>> GetSegmentIDBasedOnSegmentName(string segmentName);
		Task<ServiceResult<SegmentsDto>> GetSegmentsBasedOnSegmentId(string segmentId);
		Task<ServiceResult<PaginationDetails<SegmentsDto>>> GetSegmentsFromDB(int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
		Task<ServiceResult<SegmentsDto>> UpdateSegment(SegmentsDto s);
		Task<ServiceResult<PaginationDetails<SegmentsDto>>> SearchSegmentsFromDB(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
		Task<ServiceResult<ImportResultSummary>> ImportSegmentsFromExcel(ImportDataDto p);
		Task<ServiceResult<MemoryStream>> GetSegmentsForCSVExportBySelectedFields(List<string> selectedColumnNames);
	}
}