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
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
    public class SupplierDAL : ISupplierDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ILogger<SupplierDAL> _logger;
        private readonly IExcelDomainService _excelDomainService;

        public SupplierDAL(ILogger<SupplierDAL> logger, AssetlenDbContext context, IExcelDomainService excelDomainService)
        {
            _logger = logger;
            _context = context;
            _excelDomainService = excelDomainService;
        }

        #region Method for adding Supplier to the DB
        public async Task<ServiceResult<SupplierDto>> AddSupplierToDB([Required] SupplierDto supplierDto)
        {
            try
            {
                var supplier = supplierDto.Adapt<tbl_Supplier>();
                await _context.tbl_Suppliers.AddAsync(supplier);
                await _context.SaveChangesAsync();
                return ServiceResult<SupplierDto>.Success(supplier.Adapt<SupplierDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating supplier {FullName}: {Error}", supplierDto.FullName, ex);
                return ServiceResult<SupplierDto>.Failure(
                    new ServerErrorException("Could not create supplier."));
            }
        }
        #endregion

        #region Read Supplier from Database
        public async Task<ServiceResult<PaginationDetails<SupplierDto>>> GetSUpplierFromDB(int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var suppliers = await _context.tbl_Suppliers.AsNoTracking().OrderBy(c => c.FullName).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var suppliersDto = suppliers.Adapt<PaginationDetails<SupplierDto>>();

                return ServiceResult<PaginationDetails<SupplierDto>>.Success(suppliersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching suppliers from database: {Error}", ex);
                return ServiceResult<PaginationDetails<SupplierDto>>.Failure(
                    new ServerErrorException("Could not fetch suppliers."));
            }
        }
        #endregion

        #region Read Supplier from Database for ComboBoxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchSupplierFromDbForComboBoxes(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                IQueryable<tbl_Supplier> query = _context.tbl_Suppliers;

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(x => x.Id.ToString().Contains(keywords) ||
                                        x.FullName != null && x.FullName.ToLower().Contains(keywords.ToLower()) ||
                                        x.AccountNumber != null && x.AccountNumber.ToLower().Contains(keywords.ToLower()) ||
                                        x.Contact != null && x.Contact.ToLower().Contains(keywords.ToLower()) ||
                                        x.Email != null && x.Email.ToLower().Contains(keywords.ToLower()) ||
                                        x.VatNumber != null && x.VatNumber.ToLower().Contains(keywords.ToLower()) ||
                                        x.Company != null && x.Company.ToLower().Contains(keywords.ToLower())
                    );
                }

                var result = await query.AsNoTracking()
                    .Select(x => new ComboBoxDto
                    {
                        Id = x.Id,
                        IdString = x.Id.ToString(),
                        ValueText = x.FullName ?? string.Empty,
                    }).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching suppliers for combobox with keywords '{Keywords}': {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException("Could not search suppliers."));
            }
        }
        #endregion

        #region Get Suppliers from Database based on SupplierID
        public async Task<ServiceResult<SupplierDto>> GetSuppliersFromDBbasedOnSuplierID(string supplierId)
        {
            try
            {
                var result = await _context.tbl_Suppliers.FirstOrDefaultAsync(x => x.Id.Equals(supplierId));
                if (result == null)
                {
                    _logger.LogError("Supplier with ID: {SupplierId} not found.", supplierId);
                    return ServiceResult<SupplierDto>.Failure(
                        new NotFoundException($"Supplier with Id {supplierId} not found"));
                }
                return ServiceResult<SupplierDto>.Success(result.Adapt<SupplierDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching supplier with ID {SupplierId}: {Error}", supplierId, ex);
                return ServiceResult<SupplierDto>.Failure(
                    new ServerErrorException("Could not fetch supplier."));
            }
        }
        #endregion

        #region Get SupplierID from Database based on SupplierFullName
        public async Task<ServiceResult<string>> GetSupplierIDFromDBbasedOnSuplierName(string supplierName)
        {
            try
            {
                var result = await _context.tbl_Suppliers.FirstOrDefaultAsync(x => x.FullName.ToLower().Contains(supplierName));
                if (result == null)
                {
                    _logger.LogError("Supplier with FullName: {SupplierName} not found.", supplierName);
                    return ServiceResult<string>.Failure(
                        new NotFoundException($"Supplier with FullName {supplierName} not found"));
                }
                return ServiceResult<string>.Success(result.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching supplier with FullName {SupplierName}: {Error}", supplierName, ex);
                return ServiceResult<string>.Failure(
                    new ServerErrorException("Could not fetch supplier."));
            }
        }
        #endregion

        #region update supplier in the DB
        public async Task<ServiceResult<SupplierDto>> UpdateSupplierUsingSupplierID(SupplierDto supplierDto)
        {
            try
            {
                var supplier = supplierDto.Adapt<tbl_Supplier>();
                var objFromDb = await _context.tbl_Suppliers.FirstOrDefaultAsync(x => x.Id == supplier.Id);
                if (objFromDb == null)
                {
                    _logger.LogError("Supplier with id {SupplierId} was not found for update.", supplier.Id);
                    return ServiceResult<SupplierDto>.Failure(
                        new NotFoundException($"Supplier with id {supplier.Id}  was not found"));
                }

                objFromDb.VatNumber = supplier.VatNumber ?? objFromDb.VatNumber;
                objFromDb.AccountNumber = supplier.AccountNumber ?? objFromDb.AccountNumber;
                objFromDb.CardNumber = supplier.CardNumber ?? objFromDb.CardNumber;
                objFromDb.Address = supplier.Address ?? objFromDb.Address;
                objFromDb.Company = supplier.Company ?? objFromDb.Company;
                objFromDb.Contact = supplier.Contact ?? objFromDb.Contact;
                objFromDb.CreditLimit = supplier.CreditLimit ?? objFromDb.CreditLimit;
                objFromDb.Email = supplier.Email ?? objFromDb.Email;
                objFromDb.FullName = supplier.FullName ?? objFromDb.FullName;

                await _context.SaveChangesAsync();
                return ServiceResult<SupplierDto>.Success(objFromDb.Adapt<SupplierDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating supplier {FullName}: {Error}", supplierDto.FullName, ex);
                return ServiceResult<SupplierDto>.Failure(
                    new ServerErrorException("Could not update supplier."));
            }
        }
        #endregion

        #region search Supplier from Database based On Keywords

        public async Task<ServiceResult<PaginationDetails<SupplierDto>>> SearchSupplierUsingKeywords(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var result = await _context.tbl_Suppliers
                    .Where(x => x.Id.ToString().Contains(keywords) ||
                    x.FullName != null && x.FullName.ToLower().Contains(keywords.ToLower()) ||
                    x.AccountNumber != null && x.AccountNumber.ToLower().Contains(keywords.ToLower()) ||
                    x.Contact != null && x.Contact.ToLower().Contains(keywords.ToLower()) ||
                    x.Email != null && x.Email.ToLower().Contains(keywords.ToLower()) ||
                    x.VatNumber != null && x.VatNumber.ToLower().Contains(keywords.ToLower()) ||
                    x.Company != null && x.Company.ToLower().Contains(keywords.ToLower())
                    ).AsNoTracking()
                    .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                var resultDto = result.Adapt<PaginationDetails<SupplierDto>>();
                return ServiceResult<PaginationDetails<SupplierDto>>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching suppliers with keywords '{Keywords}': {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<SupplierDto>>.Failure(
                    new ServerErrorException("Could not search suppliers."));
            }

        }

        #endregion

        #region Delete Customer softdelete
        public async Task<ServiceResult<bool>> deleteSuppierUsingSupplierID(string supplierId)
        {
            try
            {
                var objFromDb = await _context.tbl_Suppliers.FirstOrDefaultAsync(x => x.Id == supplierId);
                if (objFromDb == null)
                {
                    _logger.LogError("Supplier with id {SupplierId} was not found for deletion.", supplierId);
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Supplier with id {supplierId}  was not found"));
                }

                objFromDb.IsDeleted = true;
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting supplier with ID {SupplierId}: {Error}", supplierId, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete supplier."));
            }
        }
        #endregion

        #region Get Suppliers ForCSVExport BasedOn SelectedFields
        public async Task<ServiceResult<MemoryStream>> GetSuppliersForCSVExportBySelectedFields(List<string> selectedColumnNames)
        {
            try
            {
                IQueryable<tbl_Supplier> query = _context.tbl_Suppliers;

                // Build the dynamic SELECT clause
                var selectFields = new List<string>();
                var properties = typeof(tbl_Supplier).GetProperties();
                foreach (var prop in properties)
                {
                    if (selectedColumnNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                        selectFields.Add(prop.Name);
                }

                // Project only the selected columns dynamically
                var dynamicQuery = query.Select($"new ({string.Join(", ", selectFields)})");

                var exportObject = dynamicQuery.Adapt<List<SupplierExportDto>>();
                //create excel file and return it
                var memorystream = await _excelDomainService.ExportExcelRecords(exportObject, selectedColumnNames, "Suppliers");

                await Task.CompletedTask;

                return ServiceResult<MemoryStream>.Success(memorystream);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while exporting suppliers: {Error}", ex);
                return ServiceResult<MemoryStream>.Failure(
                    new ServerErrorException("Could not export suppliers."));
            }
        }
        #endregion

        #region Import Suppliers from Excel
        public async Task<ServiceResult<ImportResultSummary>> ImportSuppliersFromExcel(ImportDataDto p)
        {
            if (p == null)
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("Import data is required."));

            int totalSuppliers = 0;
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
                totalSuppliers++;
                try
                {
                    tbl_Supplier? supplierEntity = null;
                    bool isUpdate = false;

                    if (p.ColumnMappingsList == null || p.ColumnMappingsList.Count == 0)
                    {
                        return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No column mappings provided."));
                    }
                    var supplierNameKey = GetKey("FullName", p.ColumnMappingsList);
                    string supplierName = !string.IsNullOrEmpty(supplierNameKey) ? GetValue(catInList, supplierNameKey)?.ToString()! : string.Empty;
                    if (string.IsNullOrEmpty(supplierName))
                    {
                        string description = BuildSupplierDescription(catInList, p);
                        messages.Add($"&#x1F4CC; {description} could not be processed due to missing Supplier name.");
                        failedCount++;
                        continue;
                    }
                    supplierEntity = await _context.tbl_Suppliers.FirstOrDefaultAsync(c => c.FullName == supplierName);

                    if (supplierEntity != null)
                    {
                        isUpdate = true;
                    }
                    else
                    {
                        // Check if name is unique (for creation)
                        bool exists = await _context.tbl_Suppliers.AnyAsync(c => c.FullName == supplierName);
                        if (exists)
                        {
                            string description = BuildSupplierDescription(catInList, p);
                            messages.Add($"&#x1F4CC; {description} could not be added as the supplier name '{supplierName}' already exists.");
                            failedCount++;
                            continue;
                        }
                        // Create new supplier
                        supplierEntity = new tbl_Supplier();
                        _context.tbl_Suppliers.Add(supplierEntity);
                        isUpdate = false;
                    }

                    // Map fields from Excel data to supplier entity
                    foreach (var mapping in p.ColumnMappingsList)
                    {
                        string systemColumn = mapping.SystemColumn.ToLower();
                        string fileColumn = mapping.SelectedFileColumn;
                        if (string.IsNullOrEmpty(fileColumn))
                            continue;

                        object value = GetValue(catInList, fileColumn);

                        switch (systemColumn)
                        {
                            case "supplierid":
                                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                                    supplierEntity.Id = value.ToString();
                                break;
                            case "accountnumber":
                                supplierEntity.AccountNumber = value?.ToString();
                                break;
                            case "fullname":
                                supplierEntity.FullName = value?.ToString();
                                break;
                            case "contact":
                                supplierEntity.Contact = value?.ToString();
                                break;
                            case "cardnumber":
                                supplierEntity.CardNumber = value?.ToString();
                                break;
                            case "vatnumber":
                                supplierEntity.VatNumber = value?.ToString();
                                break;
                            case "email":
                                supplierEntity.Email = value?.ToString();
                                break;
                            case "address":
                                supplierEntity.Address = value?.ToString();
                                break;
                            case "creditlimit":
                                if (value != null && decimal.TryParse(value.ToString(), out decimal creditLimit))
                                    supplierEntity.CreditLimit = creditLimit;
                                break;
                            case "deleted":
                                if (value != null && bool.TryParse(value.ToString(), out bool deleted))
                                    supplierEntity.Deleted = deleted;
                                break;
                            case "company":
                                supplierEntity.Company = value?.ToString();
                                break;
                                // SupplierId is handled above and not mapped here if needed
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
                    string description = BuildSupplierDescription(catInList, p);
                    _logger.LogError("Error while importing supplier: {Description}. Error: {Error}", description, ex);
                    messages.Add($"&#x1F4CC; {description} could not be imported due to an error.");
                    failedCount++;
                }
            }

            string summary = $"Total Suppliers Processed: {totalSuppliers}\n\nCreated: {createdCount}\nUpdated: {updatedCount}\nFailed: {failedCount}";
            string resultMessage = string.Join("\n", messages);

            var output = new ImportResultSummary
            {
                Summary = summary,
                Errors = resultMessage
            };

            return ServiceResult<ImportResultSummary>.Success(output);
        }
        private string BuildSupplierDescription(Dictionary<string, object> supplierData, ImportDataDto p)
        {
            List<string> parts = new List<string>();

            var supplierIdKey = GetKey("SupplierId", p.ColumnMappingsList);
            var fullNameKey = GetKey("FullName", p.ColumnMappingsList);
            var accountNumberKey = GetKey("AccountNumber", p.ColumnMappingsList);
            var contactKey = GetKey("Contact", p.ColumnMappingsList);
            var cardNumberKey = GetKey("CardNumber", p.ColumnMappingsList);
            var vatNumberKey = GetKey("VatNumber", p.ColumnMappingsList);
            var emailKey = GetKey("Email", p.ColumnMappingsList);
            var addressKey = GetKey("Address", p.ColumnMappingsList);
            var creditLimitKey = GetKey("CreditLimit", p.ColumnMappingsList);
            var deletedKey = GetKey("Deleted", p.ColumnMappingsList);
            var companyKey = GetKey("Company", p.ColumnMappingsList);

            var supplierIdVal = !string.IsNullOrEmpty(supplierIdKey) ? GetValue(supplierData, supplierIdKey)?.ToString() : "";
            var fullNameVal = !string.IsNullOrEmpty(fullNameKey) ? GetValue(supplierData, fullNameKey)?.ToString() : "";
            var accountNumberVal = !string.IsNullOrEmpty(accountNumberKey) ? GetValue(supplierData, accountNumberKey)?.ToString() : "";
            var contactVal = !string.IsNullOrEmpty(contactKey) ? GetValue(supplierData, contactKey)?.ToString() : "";
            var cardNumberVal = !string.IsNullOrEmpty(cardNumberKey) ? GetValue(supplierData, cardNumberKey)?.ToString() : "";
            var vatNumberVal = !string.IsNullOrEmpty(vatNumberKey) ? GetValue(supplierData, vatNumberKey)?.ToString() : "";
            var emailVal = !string.IsNullOrEmpty(emailKey) ? GetValue(supplierData, emailKey)?.ToString() : "";
            var addressVal = !string.IsNullOrEmpty(addressKey) ? GetValue(supplierData, addressKey)?.ToString() : "";
            var creditLimitVal = !string.IsNullOrEmpty(creditLimitKey) ? GetValue(supplierData, creditLimitKey)?.ToString() : "";
            var deletedVal = !string.IsNullOrEmpty(deletedKey) ? GetValue(supplierData, deletedKey)?.ToString() : "";
            var companyVal = !string.IsNullOrEmpty(companyKey) ? GetValue(supplierData, companyKey)?.ToString() : "";

            if (!string.IsNullOrEmpty(supplierIdVal))
                parts.Add($"SupplierId: {supplierIdVal}");
            if (!string.IsNullOrEmpty(fullNameVal))
                parts.Add($"FullName: {fullNameVal}");
            if (!string.IsNullOrEmpty(accountNumberVal))
                parts.Add($"AccountNumber: {accountNumberVal}");
            if (!string.IsNullOrEmpty(contactVal))
                parts.Add($"Contact: {contactVal}");
            if (!string.IsNullOrEmpty(cardNumberVal))
                parts.Add($"CardNumber: {cardNumberVal}");
            if (!string.IsNullOrEmpty(vatNumberVal))
                parts.Add($"VatNumber: {vatNumberVal}");
            if (!string.IsNullOrEmpty(emailVal))
                parts.Add($"Email: {emailVal}");
            if (!string.IsNullOrEmpty(addressVal))
                parts.Add($"Address: {addressVal}");
            if (!string.IsNullOrEmpty(creditLimitVal))
                parts.Add($"CreditLimit: {creditLimitVal}");
            if (!string.IsNullOrEmpty(deletedVal))
                parts.Add($"Deleted: {deletedVal}");
            if (!string.IsNullOrEmpty(companyVal))
                parts.Add($"Company: {companyVal}");

            return "Supplier [" + string.Join(", ", parts) + "]";
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
