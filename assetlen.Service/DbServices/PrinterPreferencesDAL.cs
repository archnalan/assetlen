using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Service.Domain.Interfaces;
using assetlen.Service.Extensions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace assetlen.Service.DbServices
{
    public class PrinterPreferencesDAL : IPrinterPreferencesDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ILogger<PrinterPreferencesDAL> _logger;

        public PrinterPreferencesDAL(AssetlenDbContext context, ILogger<PrinterPreferencesDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> GetPrinterPreferences(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                IQueryable<tbl_PrinterPreferances> query = _context.tbl_PrinterPreferances;

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.Id.ToString().Contains(keywords) ||
                                             c.PrinterName.Contains(keywords) ||
                                             c.ReceiptSlipType.Equals(keywords));
                }

                var preferences = await query.AsNoTracking()
                                        .Select(x => new ComboBoxDto
                                        {
                                            Id = x.Id,
                                            IdString = x.Id.ToString(),
                                            ValueText = x.PrinterName
                                        })
                                        .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(preferences);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer preferences");
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(new ServerErrorException("Internal server error"));
            }
        }

        public async Task<ServiceResult<PrinterPreferancesDto>> AddOrUpdatePrinterPreferences(PrinterPreferancesDto printerPreferences)
        {
            try
            {
                if (printerPreferences.Id == "default")
                {
                    var prefs = await _context.tbl_PrinterPreferances.Where(x => x.ReceiptSlipType == printerPreferences.ReceiptSlipType).ToListAsync();
                    _context.tbl_PrinterPreferances.RemoveRange(prefs);
                    await _context.SaveChangesAsync();
                    return ServiceResult<PrinterPreferancesDto>.Success(printerPreferences);
                }
                var existing = string.IsNullOrEmpty(printerPreferences.Id) ? null : await _context.tbl_PrinterPreferances
                    .FindAsync(printerPreferences.Id);

                if (existing == null)
                {
                    var newPreference = printerPreferences.Adapt<tbl_PrinterPreferances>();
                    newPreference.Id = Guid.NewGuid().ToString();
                    _context.tbl_PrinterPreferances.Add(newPreference);
                }
                else
                {
                    existing.PrinterName = printerPreferences.PrinterName;
                    existing.ReceiptSlipType = printerPreferences.ReceiptSlipType;
                }

                await _context.SaveChangesAsync();
                return ServiceResult<PrinterPreferancesDto>.Success(printerPreferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving printer preferences");
                return ServiceResult<PrinterPreferancesDto>.Failure(new ServerErrorException("Internal server error"));
            }
        }

        public async Task<ServiceResult<bool>> DeletePrinterPreferences(string id)
        {
            try
            {
                var preference = await _context.tbl_PrinterPreferances
                    .FindAsync(id);

                if (preference == null)
                    return ServiceResult<bool>.Failure(new NotFoundException("Printer preferences not found"));

                preference.IsDeleted = true; //soft delete
                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting printer preferences");
                return ServiceResult<bool>.Failure(new ServerErrorException("Internal server error"));
            }
        }

        public async Task<ServiceResult<PrinterPreferancesDto>> GetPrinterPreferencesById(string id)
        {
            try
            {
                var preferences = await _context.tbl_PrinterPreferances
                    .FindAsync(id);

                if (preferences == null)
                    return ServiceResult<PrinterPreferancesDto>.Failure(new NotFoundException("Printer preferences not found"));

                var result = preferences.Adapt<PrinterPreferancesDto>();
                return ServiceResult<PrinterPreferancesDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer preferences by id");
                return ServiceResult<PrinterPreferancesDto>.Failure(new ServerErrorException("Internal server error"));
            }
        }
        public async Task<ServiceResult<PrinterPreferancesDto>> GetPrinterPreferencesBySlipType(int slipTypeId)
        {
            try
            {
                var preferences = await _context.tbl_PrinterPreferances.OrderByDescending(x => x.DateTimeModified)
                    .FirstOrDefaultAsync(x => x.ReceiptSlipType == slipTypeId);

                if (preferences == null)
                    return ServiceResult<PrinterPreferancesDto>.Failure(new NotFoundException("Printer preferences not found"));

                var result = preferences.Adapt<PrinterPreferancesDto>();
                return ServiceResult<PrinterPreferancesDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting printer preferences by sliptypeID");
                return ServiceResult<PrinterPreferancesDto>.Failure(new ServerErrorException("Internal server error"));
            }
        }
    }
}

