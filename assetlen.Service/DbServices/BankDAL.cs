using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;
using System.Linq.Dynamic.Core;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace assetlen.Service.DbServices
{
    public class BankDAL : IBankDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<BankDAL> _logger;
        private readonly ITenantProvider _tenantProvider;
        private readonly IExcelDomainService _excelDomainService;

        public BankDAL(ILogger<BankDAL> logger, mowtDbContext context, ITenantProvider tenantProvider, IExcelDomainService excelDomainService)
        {
            _logger = logger;
            _context = context;
            _tenantProvider = tenantProvider;
            _excelDomainService = excelDomainService;
        }

        #region Read Banks from Database
        public async Task<ServiceResult<PaginationDetails<BankDto>>> GetBanksFromDB(int offSet, int limit, CancellationToken cancellationToken, string? sortByColumn, bool sortAscending)
        {
            try
            {
                var banks = await _context.tbl_Banks.AsNoTracking().OrderBy(b => b.BankName).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var banksDto = banks.Adapt<PaginationDetails<BankDto>>();

                return ServiceResult<PaginationDetails<BankDto>>.Success(banksDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching banks from database: {Error}", ex);
                return ServiceResult<PaginationDetails<BankDto>>.Failure(
                    new ServerErrorException("Could not fetch banks."));
            }
        }
        #endregion

        #region Read BankID from Database based on BankName
        public async Task<ServiceResult<string>> GetBankIDBasedOnBankName(string bankName)
        {
            try
            {
                string sql = "select Id from tbl_Banks where IsDeleted=0 and BankName = @bankName";

                var result = await _context.tbl_Banks.FromSqlRaw(sql, new SqlParameter("@bankName", bankName))
                                                            .Select(b => b.Id).FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(result))
                {
                    _logger.LogError("Bank with name: {BankName} not found.", bankName);
                    return ServiceResult<string>.Failure(new NotFoundException($"Bank with name {bankName} not found."));
                }

                return ServiceResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching bank with name {BankName}: {Error}", bankName, ex);
                return ServiceResult<string>.Failure(
                    new ServerErrorException("Could not fetch bank."));
            }
        }
        #endregion

        #region Read Banks from Database for ComboBoxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchBanksFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            var query = _context.tbl_Banks.AsNoTracking();
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(b => b.Id.ToString().Contains(keywords)
                    || b.BankName != null && b.BankName.ToLower().Contains(keywords.ToLower())
                    || b.SwiftCode != null && b.SwiftCode.ToLower().Contains(keywords.ToLower()));
                }
                var banks = await query.AsNoTracking()
                                       .Select(x => new ComboBoxDto
                                       {
                                           Id = x.Id,
                                           IdString = x.Id.ToString(),
                                           ValueText = x.BankName ?? string.Empty
                                       })
                                       .ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(banks);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching banks for combobox: {Error}", ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException("Could not search banks."));
            }
        }
        #endregion

        #region Add Bank to DB
        public async Task<ServiceResult<BankDto>> AddBank(BankDto b)
        {
            if (b == null) return ServiceResult<BankDto>.Failure(
                                new BadRequestException("Bank data is required."));

            var bankExists = await _context.tbl_Banks.AnyAsync(x => x.BankName == b.BankName);

            if (bankExists) return ServiceResult<BankDto>.Failure(
                                    new ConflictException($"Bank {b.BankName} already exists."));

            try
            {
                var bank = b.Adapt<tbl_Bank>();

                await _context.AddAsync(bank);

                await _context.SaveChangesAsync();

                var createdBank = bank.Adapt<BankDto>();

                return ServiceResult<BankDto>.Success(createdBank);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating bank: {Error}", ex);
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
                {
                    string errorMessage = "The Bank you are trying to create already exists in this system. Please choose another name.";
                    return ServiceResult<BankDto>.Failure(new BadRequestException(errorMessage));
                }

                return ServiceResult<BankDto>.Failure(
                    new ServerErrorException("Could not create bank."));
            }
        }
        #endregion

        #region Get Bank from Database based on BankID
        public async Task<ServiceResult<BankDto>> GetBankById(string id)
        {
            try
            {
                var bank = await _context.tbl_Banks.FindAsync(id);

                if (bank == null)
                {
                    _logger.LogError("Bank with ID: {BankId} not found.", id);
                    return ServiceResult<BankDto>.Failure(
                        new NotFoundException($"Bank with ID: {id} not found."));
                }

                return ServiceResult<BankDto>.Success(bank.Adapt<BankDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching bank with ID {BankId}: {Error}", id, ex);
                return ServiceResult<BankDto>.Failure(
                    new ServerErrorException("Could not fetch bank."));
            }
        }
        #endregion

        #region Get Bank from Database based on BankName
        public async Task<ServiceResult<BankDto>> GetBankByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return ServiceResult<BankDto>.Failure(
                                                new BadRequestException("Bank name is required."));

            try
            {
                var bank = await _context.tbl_Banks.FirstOrDefaultAsync(b => b.BankName == name);

                if (bank == null)
                {
                    _logger.LogError("Bank with name: {BankName} not found.", name);
                    return ServiceResult<BankDto>.Failure(
                        new NotFoundException($"Bank with name: {name} not found."));
                }

                return ServiceResult<BankDto>.Success(bank.Adapt<BankDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching bank with name {BankName}: {Error}", name, ex);
                return ServiceResult<BankDto>.Failure(
                    new ServerErrorException("Could not fetch bank."));
            }
        }
        #endregion

        #region Update bank in the DB
        public async Task<ServiceResult<BankDto>> UpdateBank(BankDto bDto)
        {
            if (bDto == null) return ServiceResult<BankDto>.Failure(
                                new BadRequestException("Bank data is required."));

            var bankInDb = await _context.tbl_Banks.FirstOrDefaultAsync(b => b.Id == bDto.Id);

            if (bankInDb == null) return ServiceResult<BankDto>.Failure(
                                    new NotFoundException($"Bank with ID {bDto.Id} not found."));

            try
            {
                //Map the incoming data excluding unchanged properties
                bankInDb.BankName = bDto.BankName ?? bankInDb.BankName;
                bankInDb.SwiftCode = bDto.SwiftCode ?? bankInDb.SwiftCode;
                bankInDb.Address = bDto.Address ?? bankInDb.Address;
                bankInDb.IsActive = bDto.IsActive;
                bankInDb.Description = bDto.Description ?? bankInDb.Description;

                await _context.SaveChangesAsync();

                return ServiceResult<BankDto>.Success(bankInDb.Adapt<BankDto>());

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating bank with ID {BankName}: {Error}", bDto.BankName, ex);
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
                {
                    string errorMessage = "The Bank you are trying to update already exists in this system. Please choose another name.";
                    return ServiceResult<BankDto>.Failure(new BadRequestException(errorMessage));
                }

                return ServiceResult<BankDto>.Failure(
                    new ServerErrorException("Could not update bank."));
            }
        }
        #endregion

        #region Delete bank softdelete
        public async Task<ServiceResult<bool>> DeleteBankById(string id)
        {
            try
            {
                var bankInDb = await _context.tbl_Banks.FindAsync(id);

                if (bankInDb == null)
                {
                    _logger.LogError("Bank with ID: {BankId} not found for deletion.", id);
                    return ServiceResult<bool>
                        .Failure(new NotFoundException($"Bank with ID: {id} not found."));
                }

                //soft delete
                bankInDb.IsDeleted = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Bank with ID {id} could not be deleted.: {ex}", id, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete bank."));
            }
        }
        #endregion

        #region Search Banks from Database
        public async Task<ServiceResult<PaginationDetails<BankDto>>> SearchBanksFromDB(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            var query = _context.tbl_Banks.AsNoTracking();
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(b => b.Id.ToString().Contains(keywords)
                    || b.BankName != null && b.BankName.ToLower().Contains(keywords.ToLower())
                    || b.SwiftCode != null && b.SwiftCode.ToLower().Contains(keywords.ToLower()));
                }
                var banks = await query.ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

                var banksDto = banks.Adapt<PaginationDetails<BankDto>>();

                return ServiceResult<PaginationDetails<BankDto>>.Success(banksDto);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching banks: {Error}", ex);
                return ServiceResult<PaginationDetails<BankDto>>.Failure(
                    new ServerErrorException("Could not search banks."));
            }
        }
        #endregion

        #region Get Banks ForCSVExport BasedOn SelectedFields
        public async Task<ServiceResult<MemoryStream>> GetBanksForCSVExportBySelectedFields(List<string> selectedColumnNames)
        {
            try
            {
                IQueryable<tbl_Bank> query = _context.tbl_Banks;

                // Build the dynamic SELECT clause
                var selectFields = new List<string>();
                var properties = typeof(tbl_Bank).GetProperties();
                foreach (var prop in properties)
                {
                    if (selectedColumnNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                        selectFields.Add(prop.Name);
                }

                // Project only the selected columns dynamically
                var dynamicQuery = query.Select($"new ({string.Join(", ", selectFields)})");

                var exportObject = dynamicQuery.Adapt<List<BanksExportDto>>();
                //create excel file and return it
                var memorystream = await _excelDomainService.ExportExcelRecords(exportObject, selectedColumnNames, "Banks");

                return ServiceResult<MemoryStream>.Success(memorystream);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while exporting banks: {Error}", ex);
                return ServiceResult<MemoryStream>.Failure(
                    new ServerErrorException("Could not export banks."));
            }
        }
        #endregion

        #region Import Banks from Excel
        public async Task<ServiceResult<ImportResultSummary>> ImportBanksFromExcel(ImportDataDto p)
        {
            if (p == null)
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("Import data is required."));

            int totalBanks = 0;
            int updatedCount = 0;
            int createdCount = 0;
            int failedCount = 0;
            List<string> messages = new List<string>();

            if (p.UploadedExcelContent == null || p.UploadedExcelContent.Count == 0)
            {
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No data found in the uploaded file."));
            }

            foreach (var bankInList in p.UploadedExcelContent)
            {
                totalBanks++;
                try
                {
                    tbl_Bank? bankEntity = null;
                    bool isUpdate = false;

                    if (p.ColumnMappingsList == null || p.ColumnMappingsList.Count == 0)
                    {
                        return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No column mappings provided."));
                    }

                    var bankNameKey = GetKey("BankName", p.ColumnMappingsList);
                    string bankName = !string.IsNullOrEmpty(bankNameKey) ? GetValue(bankInList, bankNameKey)?.ToString()! : string.Empty;

                    if (string.IsNullOrEmpty(bankName))
                    {
                        string description = BuildBankDescription(bankInList, p);
                        messages.Add($"&#x1F4CC; {description} could not be processed due to missing Bank name.");
                        failedCount++;
                        continue;
                    }

                    bankEntity = await _context.tbl_Banks.FirstOrDefaultAsync(b => b.BankName == bankName);
                    if (bankEntity != null)
                    {
                        isUpdate = true;
                    }
                    else
                    {
                        // Check if name is unique (for creation)
                        bool exists = await _context.tbl_Banks.AnyAsync(b => b.BankName == bankName);
                        if (exists)
                        {
                            string description = BuildBankDescription(bankInList, p);
                            messages.Add($"&#x1F4CC; {description} could not be added as the bank name '{bankName}' already exists.");
                            failedCount++;
                            continue;
                        }
                        // Create new bank
                        bankEntity = new tbl_Bank();
                        _context.tbl_Banks.Add(bankEntity);
                        isUpdate = false;
                    }

                    // Map fields from Excel data to bank entity
                    foreach (var mapping in p.ColumnMappingsList)
                    {
                        string systemColumn = mapping.SystemColumn.ToLower();
                        string fileColumn = mapping.SelectedFileColumn;
                        if (string.IsNullOrEmpty(fileColumn))
                            continue;

                        object value = GetValue(bankInList, fileColumn);

                        switch (systemColumn)
                        {
                            case "bankname":
                                bankEntity.BankName = value?.ToString();
                                break;
                                break;
                            case "swiftcode":
                                bankEntity.SwiftCode = value?.ToString();
                                break;
                            case "address":
                                bankEntity.Address = value?.ToString();
                                break;
                            case "isactive":
                                if (value != null && bool.TryParse(value.ToString(), out bool isActive))
                                    bankEntity.IsActive = isActive;
                                break;
                            case "description":
                                bankEntity.Description = value?.ToString();
                                break;
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
                    string description = BuildBankDescription(bankInList, p);
                    _logger.LogError("Error while importing bank: {Description}. Error: {Error}", description, ex);
                    messages.Add($"&#x1F4CC; {description} could not be imported due to an error.");
                    failedCount++;
                }
            }

            string summary = $"Total Banks Processed: {totalBanks}\n\nCreated: {createdCount}\nUpdated: {updatedCount}\nFailed: {failedCount}";
            string resultMessage = string.Join("\n", messages);

            var output = new ImportResultSummary
            {
                Summary = summary,
                Errors = resultMessage
            };

            return ServiceResult<ImportResultSummary>.Success(output);
        }

        private string BuildBankDescription(Dictionary<string, object> bankData, ImportDataDto p)
        {
            List<string> parts = new List<string>();

            var bankIdKey = GetKey("Id", p.ColumnMappingsList);
            var bankNameKey = GetKey("BankName", p.ColumnMappingsList);
            var bankDateKey = GetKey("BankingDate", p.ColumnMappingsList);
            var bankCodeKey = GetKey("BankCode", p.ColumnMappingsList);

            var bankIdVal = !string.IsNullOrEmpty(bankIdKey) ? GetValue(bankData, bankIdKey)?.ToString() : "";
            var bankNameVal = !string.IsNullOrEmpty(bankNameKey) ? GetValue(bankData, bankNameKey)?.ToString() : "";
            var bankCodeVal = !string.IsNullOrEmpty(bankCodeKey) ? GetValue(bankData, bankCodeKey)?.ToString() : "";

            if (!string.IsNullOrEmpty(bankIdVal))
                parts.Add($"ID: {bankIdVal}");
            if (!string.IsNullOrEmpty(bankNameVal))
                parts.Add($"Name: {bankNameVal}");
            if (!string.IsNullOrEmpty(bankCodeVal))
                parts.Add($"Code: {bankCodeVal}");

            return "Bank [" + string.Join(", ", parts) + "]";
        }

        private object GetValue(Dictionary<string, object> item, string key)
        {
            return item.TryGetValue(key, out object? value) ? (value == null ? "" : value) : "";
        }

        private string GetKey(string columnName, List<ColumnMapping> mappings)
        {
            return mappings.FirstOrDefault(x => x.SystemColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))?.SelectedFileColumn ?? string.Empty;
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