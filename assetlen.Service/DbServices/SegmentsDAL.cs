using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.Service.FileProcessingServices;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace assetlen.Service.DbServices
{
    public class SegmentsDAL : ISegmentsDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ILogger<SegmentsDAL> _logger;
        private readonly IExcelDomainService _excelDomainService;
        public SegmentsDAL(ILogger<SegmentsDAL> logger, AssetlenDbContext context, IExcelDomainService excelDomainService)
        {
            _logger = logger;
            _context = context;
            _excelDomainService = excelDomainService;
        }

        #region Read Segments from Database
        public async Task<ServiceResult<PaginationDetails<SegmentsDto>>> GetSegmentsFromDB(int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
        {
            try
            {
                var segments = await _context.tbl_Segments.AsNoTracking().OrderBy(c => c.Segment).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var segmentsDto = segments.Adapt<PaginationDetails<SegmentsDto>>();

                return ServiceResult<PaginationDetails<SegmentsDto>>.Success(segmentsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching segments from database: {Error}", ex);
                return ServiceResult<PaginationDetails<SegmentsDto>>.Failure(
                    new ServerErrorException("Could not fetch segments."));
            }
        }
        #endregion

        #region Read Segments for combo boxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchSegmentsForComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
        {
            try
            {
                IQueryable<tbl_Segment> query = _context.tbl_Segments;

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.
                             Where(x => x.Id.ToString() == keywords ||
                             x.Segment != null && x.Segment.Contains(keywords) ||
                             x.Description != null && x.Description.Contains(keywords)
                             );
                }

                var segments = await query.AsNoTracking()
                                          .Select(x => new ComboBoxDto
                                          {
                                              Id = x.Id,
                                              IdString = x.Id.ToString(),
                                              ValueText = x.Segment ?? string.Empty
                                          })
                                          .OrderBy(c => c.ValueText)
                                          .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var segmentsDto = segments.Adapt<PaginationDetails<ComboBoxDto>>();

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(segmentsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching segments for combobox with keywords '{Keywords}': {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException("Could not search segments."));
            }
        }
        #endregion

        #region Read Segments from Database basde on ID
        public async Task<ServiceResult<SegmentsDto>> GetSegmentsBasedOnSegmentId(string segmentId)
        {
            try
            {
                var result = await _context.tbl_Segments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == segmentId);

                return ServiceResult<SegmentsDto>.Success(result.Adapt<SegmentsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching segment with ID {SegmentId}: {Error}", segmentId, ex);
                return ServiceResult<SegmentsDto>.Failure(
                    new ServerErrorException("Could not fetch segment."));
            }
        }
        #endregion

        #region Read SegmenTID from Database basde on SegmentName
        public async Task<ServiceResult<string>> GetSegmentIDBasedOnSegmentName(string segmentName)
        {
            try
            {
                var result = await _context.tbl_Segments.FirstOrDefaultAsync(x => x.Segment == segmentName);

                return ServiceResult<string>.Success(result.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching segment ID for name '{SegmentName}': {Error}", segmentName, ex);
                return ServiceResult<string>.Failure(
                    new ServerErrorException("Could not fetch segment ID."));
            }
        }
        #endregion

        #region AddSegment to DB

        public async Task<ServiceResult<SegmentsDto>> AddSegment(SegmentsDto s)
        {
            if (s == null)
                return ServiceResult<SegmentsDto>.Failure(new BadRequestException("Segment data is required."));

            try
            {
                var input = s.Adapt<tbl_Segment>();
                await _context.tbl_Segments.AddAsync(input);
                await _context.SaveChangesAsync();

                return ServiceResult<SegmentsDto>.Success(input.Adapt<SegmentsDto>());
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    string errorMessage = "The segment you are trying to create already exists. Please choose another name.";
                    _logger.LogError("Attempted to create duplicate segment: {SegmentName}", s?.Segment);
                    return ServiceResult<SegmentsDto>.Failure(new BadRequestException(errorMessage));
                }
                _logger.LogError("Error while creating segment '{SegmentName}': {Error}", s?.Segment, ex);
                return ServiceResult<SegmentsDto>.Failure(
                    new ServerErrorException("Could not create segment."));
            }
        }
        #endregion

        #region update Segment into DB
        public async Task<ServiceResult<SegmentsDto>> UpdateSegment(SegmentsDto s)
        {
            if (s == null)
                return ServiceResult<SegmentsDto>.Failure(new BadRequestException("Segment data is required."));

            try
            {
                var objFromDb = await _context.tbl_Segments.FirstOrDefaultAsync(x => x.Id == s.Id);
                if (objFromDb == null)
                {
                    _logger.LogError("Segment with ID: {SegmentId} not found for update.", s.Id);
                    return ServiceResult<SegmentsDto>.Failure(
                        new NotFoundException($"Segment with ID: {s.Id} not found."));
                }

                objFromDb.HideInPos = s.HideInPos;
                objFromDb.Description = s.Description ?? objFromDb.Description;
                objFromDb.Segment = s.Segment ?? objFromDb.Segment;
                objFromDb.Description = s.Description ?? objFromDb.Description;

                await _context.SaveChangesAsync();

                return ServiceResult<SegmentsDto>.Success(objFromDb.Adapt<SegmentsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating segment '{SegmentId}': {Error}", s?.Id, ex);
                return ServiceResult<SegmentsDto>.Failure(
                    new ServerErrorException("Could not update segment."));
            }
        }
        #endregion

        #region Delete segment softdelete
        public async Task<ServiceResult<bool>> DeleteSegment(string segmentId)
        {
            try
            {
                var objFromDb = await _context.tbl_Segments.FirstOrDefaultAsync(x => x.Id == segmentId);
                if (objFromDb == null)
                {
                    _logger.LogError("Segment with ID: {SegmentId} not found for deletion.", segmentId);
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Segment with ID: {segmentId} not found."));
                }

                objFromDb.IsDeleted = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting segment with ID {SegmentId}: {Error}", segmentId, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete segment."));
            }
        }
        #endregion

        #region Search Segments from Database
        public async Task<ServiceResult<PaginationDetails<SegmentsDto>>> SearchSegmentsFromDB(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
        {
            try
            {
                IQueryable<tbl_Segment> query = _context.tbl_Segments;
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.
                             Where(x => x.Id.ToString() == keywords ||
                             x.Segment != null && x.Segment.Contains(keywords) ||
                             x.Description != null && x.Description.Contains(keywords)
                             );
                }
                var segments = await query.AsNoTracking()
                                          .OrderBy(c => c.Segment)
                                          .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                var segmentsDto = segments.Adapt<PaginationDetails<SegmentsDto>>();
                return ServiceResult<PaginationDetails<SegmentsDto>>.Success(segmentsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching segments with keywords '{Keywords}': {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<SegmentsDto>>.Failure(
                    new ServerErrorException("Could not search segments."));
            }
        }
        #endregion

        #region Get Segments ForCSVExport BasedOn SelectedFields

        public async Task<ServiceResult<MemoryStream>> GetSegmentsForCSVExportBySelectedFields(List<string> selectedColumnNames)
        {
            try
            {
                IQueryable<tbl_Segment> query = _context.tbl_Segments;

                // Build the dynamic SELECT clause
                var selectFields = new List<string>();
                var properties = typeof(tbl_Segment).GetProperties();
                foreach (var prop in properties)
                {
                    if (selectedColumnNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                        selectFields.Add(prop.Name);
                }

                // Project only the selected columns dynamically
                var dynamicQuery = query.Select($"new ({string.Join(", ", selectFields)})");

                var exportObject = dynamicQuery.Adapt<List<SegmentExportDto>>();
                //create excel file and return it
                var memorystream = await _excelDomainService.ExportExcelRecords(exportObject, selectedColumnNames, "Segments");

                await Task.CompletedTask;

                return ServiceResult<MemoryStream>.Success(memorystream);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while exporting segments: {Error}", ex);
                return ServiceResult<MemoryStream>.Failure(
                    new ServerErrorException("Could not export segments."));
            }
        }
        #endregion

        #region Import Segments from Excel
        public async Task<ServiceResult<ImportResultSummary>> ImportSegmentsFromExcel(ImportDataDto p)
        {
            if (p == null)
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("Import data is required."));

            int totalSegments = 0;
            int updatedCount = 0;
            int createdCount = 0;
            int failedCount = 0;
            List<string> messages = new List<string>();

            if (p.UploadedExcelContent == null || p.UploadedExcelContent.Count == 0)
            {
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No data found in the uploaded file."));
            }

            foreach (var catInList in p.UploadedExcelContent)
            {
                totalSegments++;
                try
                {
                    tbl_Segment? segmentEntity = null;
                    bool isUpdate = false;

                    if (p.ColumnMappingsList == null || p.ColumnMappingsList.Count == 0)
                    {
                        return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No column mappings provided."));
                    }
                    var segmentNameKey = GetKey("Segment", p.ColumnMappingsList);
                    string segmentName = !string.IsNullOrEmpty(segmentNameKey) ? GetValue(catInList, segmentNameKey)?.ToString()! : string.Empty;
                    if (string.IsNullOrEmpty(segmentName))
                    {
                        string description = BuildSegmentDescription(catInList, p);
                        messages.Add($"&#x1F4CC; {description} could not be processed due to missing segment name.");
                        failedCount++;
                        continue;
                    }
                    segmentEntity = await _context.tbl_Segments.FirstOrDefaultAsync(c => c.Segment == segmentName);
                    if (segmentEntity != null)
                    {
                        isUpdate = true;
                    }
                    else
                    {
                        // Check if name is unique (for creation)
                        bool exists = await _context.tbl_Segments.AnyAsync(c => c.Segment == segmentName);
                        if (exists)
                        {
                            string description = BuildSegmentDescription(catInList, p);
                            messages.Add($"&#x1F4CC; {description} could not be added as the segment name '{segmentName}' already exists.");
                            failedCount++;
                            continue;
                        }
                        // Create new segment
                        segmentEntity = new tbl_Segment();
                        _context.tbl_Segments.Add(segmentEntity);
                        isUpdate = false;
                    }

                    // Map fields from Excel data to segment entity
                    foreach (var mapping in p.ColumnMappingsList)
                    {
                        string systemColumn = mapping.SystemColumn.ToLower();
                        string fileColumn = mapping.SelectedFileColumn;
                        if (string.IsNullOrEmpty(fileColumn))
                            continue;

                        object value = GetValue(catInList, fileColumn);

                        switch (systemColumn)
                        {
                            case "segment":
                                segmentEntity.Segment = value?.ToString();
                                break;
                            case "description":
                                segmentEntity.Description = value?.ToString();
                                break;
                            case "hideinpos":
                                if (value != null && bool.TryParse(value.ToString(), out bool hideInPos))
                                    segmentEntity.HideInPos = hideInPos;
                                break;
                                // Segmentid is handled above and not mapped here
                        }
                    }

                    await _context.SaveChangesAsync();

                    if (isUpdate)
                        updatedCount++;
                    else
                        createdCount++;
                }
                catch (Exception ex)
                {
                    string description = BuildSegmentDescription(catInList, p);
                    _logger.LogError("Error while importing segment: {Description}. Error: {Error}", description, ex);
                    messages.Add($"&#x1F4CC; {description} could not be imported due to an error.");
                    failedCount++;
                }
            }

            string summary = $"Total Segments Processed: {totalSegments}\n\nCreated: {createdCount}\nUpdated: {updatedCount}\nFailed: {failedCount}";
            string resultMessage = string.Join("\n", messages);

            var output = new ImportResultSummary
            {
                Summary = summary,
                Errors = resultMessage
            };

            return ServiceResult<ImportResultSummary>.Success(output);
        }
        private string BuildSegmentDescription(Dictionary<string, object> catData, ImportDataDto p)
        {
            List<string> parts = new List<string>();

            var segmentIdKey = GetKey("Segmentid", p.ColumnMappingsList);
            var segmentNameKey = GetKey("Segment", p.ColumnMappingsList);
            var descriptionKey = GetKey("Description", p.ColumnMappingsList);

            var segmentIdVal = !string.IsNullOrEmpty(segmentIdKey) ? GetValue(catData, segmentIdKey)?.ToString() : "";
            var segmentNameVal = !string.IsNullOrEmpty(segmentNameKey) ? GetValue(catData, segmentNameKey)?.ToString() : "";
            var descriptionVal = !string.IsNullOrEmpty(descriptionKey) ? GetValue(catData, descriptionKey)?.ToString() : "";

            if (!string.IsNullOrEmpty(segmentIdVal))
                parts.Add($"ID: {segmentIdVal}");
            if (!string.IsNullOrEmpty(segmentNameVal))
                parts.Add($"Name: {segmentNameVal}");
            if (!string.IsNullOrEmpty(descriptionVal))
                parts.Add($"Description: {descriptionVal}");

            return "segment [" + string.Join(", ", parts) + "]";
        }
        private object GetValue(Dictionary<string, object> item, string key)
        {
            return item.TryGetValue(key, out object? value) ? (value == null ? "" : value) : "";
        }
        private string GetKey(string columnName, List<ColumnMapping> mappings)
        {
            return mappings.FirstOrDefault(x => x.SystemColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))!.SelectedFileColumn;
        }
        public static string NormalizeString(string? input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim().ToLowerInvariant();
        }

        public static bool CompareNormalizedStrings(string? str1, string? str2)
        {
            return NormalizeString(str1) == NormalizeString(str2);
        }

        #endregion
    }
}
