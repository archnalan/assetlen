using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.Service.FileProcessingServices;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Mapster;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
    public class CustomerDAL : ICustomerDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<CustomerDAL> _logger;
        private readonly IExcelDomainService _excelDomainService;

        public CustomerDAL(mowtDbContext context, ILogger<CustomerDAL> logger, IExcelDomainService excelDomainService)
        {
            _context = context;
            _logger = logger;
            _excelDomainService = excelDomainService;
        }

        #region Read Customers from Database 
        public async Task<ServiceResult<PaginationDetails<CustomerDto>>> GetCustomersFromDB(int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var customers = await _context.tbl_Customers.AsNoTracking().OrderBy(c => c.FullName).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                var customersDto = customers.Adapt<PaginationDetails<CustomerDto>>();
                return ServiceResult<PaginationDetails<CustomerDto>>.Success(customersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching customers from database: {Error}", ex);
                return ServiceResult<PaginationDetails<CustomerDto>>.Failure(
                    new ServerErrorException("Could not fetch customers."));
            }
        }
        #endregion        

        #region Get customer by ID 
        public async Task<ServiceResult<CustomerDto>> GetCustomerById(string id)
        {
            try
            {
                var customer = await _context.tbl_Customers.FindAsync(id);

                if (customer == null)
                {
                    _logger.LogError("Customer with ID: {CustomerId} not found.", id);
                    return ServiceResult<CustomerDto>.Failure(
                        new NotFoundException($"Customer with ID: {id} not found."));
                }

                return ServiceResult<CustomerDto>.Success(customer.Adapt<CustomerDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching customer with ID {CustomerId}: {Error}", id, ex);
                return ServiceResult<CustomerDto>.Failure(
                    new ServerErrorException("Could not fetch customer."));
            }
        }
        #endregion

        #region Add Customer to the DB
        public async Task<ServiceResult<CustomerDto>> AddCustomer(CustomerDto customerDto)
        {
            if (customerDto == null) return ServiceResult<CustomerDto>.Failure(
                new BadRequestException("Customer data is required."));

            try
            {
                var customer = customerDto.Adapt<tbl_Customer>();
                await _context.AddAsync(customer);
                await _context.SaveChangesAsync();
                return ServiceResult<CustomerDto>.Success(customer.Adapt<CustomerDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating customer {FullName}: {Error}", customerDto.FullName, ex);
                return ServiceResult<CustomerDto>.Failure(
                    new ServerErrorException("Could not create customer."));
            }
        }
        #endregion

        #region update Customer in the  DB

        public async Task<ServiceResult<CustomerDto>> UpdateCustomer(string id, CustomerDto updateCustomer)
        {
            if (updateCustomer == null) return ServiceResult<CustomerDto>.Failure(
                new BadRequestException("Customer data required"));

            if (updateCustomer.Id != id) return ServiceResult<CustomerDto>.Failure(
                    new BadRequestException($"Customer with ID: {id} is not the same as customer with ID: {updateCustomer.Id}"));

            var customerInDb = await _context.tbl_Customers.FirstOrDefaultAsync(c => c.Id == id);

            if (customerInDb == null) return ServiceResult<CustomerDto>.Failure(
                new NotFoundException($"Customer with ID: {id} not found."));

            try
            {
                // Updating the fields
                customerInDb.AccountNumber = updateCustomer.AccountNumber ?? customerInDb.AccountNumber;
                customerInDb.FullName = updateCustomer.FullName ?? customerInDb.FullName;
                customerInDb.Contact = updateCustomer.Contact ?? customerInDb.Contact;
                customerInDb.CardNumber = updateCustomer.CardNumber ?? customerInDb.CardNumber;
                customerInDb.VatNumber = updateCustomer.VatNumber ?? customerInDb.VatNumber;
                customerInDb.Email = updateCustomer.Email ?? customerInDb.Email;
                customerInDb.Address = updateCustomer.Address ?? customerInDb.Address;
                customerInDb.CreditLimit = updateCustomer.CreditLimit != default(decimal) ? updateCustomer.CreditLimit : customerInDb.CreditLimit;
                customerInDb.Company = updateCustomer.Company ?? customerInDb.Company;

                await _context.SaveChangesAsync();

                return ServiceResult<CustomerDto>.Success(customerInDb.Adapt<CustomerDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating customer {FullName}: {Error}", updateCustomer.FullName, ex);
                return ServiceResult<CustomerDto>.Failure(
                    new ServerErrorException("Could not update customer."));
            }
        }
        #endregion

        #region Delete Customer softdelete
        public async Task<ServiceResult<bool>> DeleteCustomerById(string id)
        {
            try
            {
                var customerInDb = await _context.tbl_Customers.FindAsync(id);

                if (customerInDb == null)
                {
                    _logger.LogError("Customer with ID: {CustomerId} not found for deletion.", id);
                    return ServiceResult<bool>
                        .Failure(new NotFoundException($"Customer with ID: {id} not found."));
                }

                //soft delete
                customerInDb.IsDeleted = true;
                customerInDb.Deleted = false;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting customer with ID {CustomerId}: {Error}", id, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete customer."));
            }
        }
        #endregion

        #region search Customers from Database based On Keywords
        public async Task<ServiceResult<PaginationDetails<CustomerDto>>> SearchCustomerByKeywords(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                if (string.IsNullOrEmpty(keywords))
                {
                    var customerResult = await GetCustomersFromDB(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                    return ServiceResult<PaginationDetails<CustomerDto>>.Success(customerResult.Data.Adapt<PaginationDetails<CustomerDto>>());
                }

                var customers = await _context.tbl_Customers.Where(c => c.Id.ToString().Contains(keywords) ||
                                                                        c.FullName != null && c.FullName.Contains(keywords) ||
                                                                        c.Contact != null && c.Contact.Contains(keywords) ||
                                                                        c.Email != null && c.Email.Contains(keywords))
                                                                        .ToPaginatedResultAsync<tbl_Customer>(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<CustomerDto>>.Success(customers.Adapt<PaginationDetails<CustomerDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching customers with keywords '{Keywords}': {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<CustomerDto>>.Failure(
                    new ServerErrorException("Could not search customers."));
            }
        }
        #endregion

        #region search Customers from Database based On Keywords
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchCustomerByKeywordsForComboBoxes(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                IQueryable<tbl_Customer> query = _context.tbl_Customers;

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.Id.ToString().Contains(keywords) ||
                                             c.FullName.Contains(keywords) ||
                                             c.Contact.Contains(keywords) ||
                                             c.Email.Contains(keywords));
                }

                var customers = await query.AsNoTracking()
                                        .Select(x => new ComboBoxDto
                                        {
                                            Id = x.Id,
                                            IdString = x.Id.ToString(),
                                            ValueText = x.FullName
                                        })
                                        .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching customers for combobox with keywords '{Keywords}': {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException("Could not search customers."));
            }
        }
        #endregion

        #region Customer Count
        public async Task<int> GetTotalCustomerCount(CancellationToken cancellation = default)
        {
            return await _context.tbl_Customers.CountAsync(cancellation);
        }
        #endregion

        #region Import Customers from Excel
        public async Task<ServiceResult<ImportResultSummary>> ImportCustomersFromExcel(ImportDataDto p)
        {
            if (p == null)
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("Import data is required."));

            int totalCustomers = 0;
            int updatedCount = 0;
            int createdCount = 0;
            int failedCount = 0;
            List<string> messages = new List<string>();

            if (p.UploadedExcelContent == null || p.UploadedExcelContent.Count == 0)
            {
                return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No data found in the uploaded file."));
            }

            foreach (var customer in p.UploadedExcelContent)
            {
                totalCustomers++;
                try
                {
                    tbl_Customer? customerEntity = null;
                    bool isUpdate = false;

                    if (p.ColumnMappingsList == null || p.ColumnMappingsList.Count == 0)
                    {
                        return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No column mappings provided."));
                    }
                    var customerNameKey = GetKey("FullName", p.ColumnMappingsList);
                    string customerName = !string.IsNullOrEmpty(customerNameKey) ? GetValue(customer, customerNameKey)?.ToString()! : string.Empty;
                    if (string.IsNullOrEmpty(customerName))
                    {
                        string description = BuildCustomerDescription(customer, p);
                        messages.Add($"&#x1F4CC; {description} could not be processed due to missing customer name.");
                        failedCount++;
                        continue;
                    }
                    customerEntity = await _context.tbl_Customers.FirstOrDefaultAsync(c => c.FullName == customerName);
                    if (customerEntity != null)
                    {
                        isUpdate = true;
                    }
                    else
                    {
                        // Check if name is unique (for creation)
                        bool exists = await _context.tbl_Customers.AnyAsync(c => c.FullName == customerName);
                        if (exists)
                        {
                            string description = BuildCustomerDescription(customer, p);
                            messages.Add($"&#x1F4CC; {description} could not be added as the customer name '{customerName}' already exists.");
                            failedCount++;
                            continue;
                        }
                        // Create new customer
                        customerEntity = new tbl_Customer();
                        _context.tbl_Customers.Add(customerEntity);
                        isUpdate = false;
                    }

                    // Map fields from Excel data to customer entity
                    foreach (var mapping in p.ColumnMappingsList)
                    {
                        string systemColumn = mapping.SystemColumn.ToLower();
                        string fileColumn = mapping.SelectedFileColumn;
                        if (string.IsNullOrEmpty(fileColumn))
                            continue;

                        object value = GetValue(customer, fileColumn);

                        switch (systemColumn)
                        {
                            case "fullname":
                                customerEntity.FullName = value?.ToString();
                                break;
                            case "accountnumber":
                                customerEntity.AccountNumber = value?.ToString();
                                break;
                            case "contact":
                                customerEntity.Contact = value?.ToString();
                                break;
                            case "cardnumber":
                                customerEntity.CardNumber = value?.ToString();
                                break;
                            case "vatnumber":
                                customerEntity.VatNumber = value?.ToString();
                                break;
                            case "email":
                                customerEntity.Email = value?.ToString();
                                break;
                            case "address":
                                customerEntity.Address = value?.ToString();
                                break;
                            case "creditlimit":
                                if (value != null && decimal.TryParse(value.ToString(), out decimal creditLimit))
                                    customerEntity.CreditLimit = creditLimit;
                                break;
                            case "deleted":
                                if (value != null && bool.TryParse(value.ToString(), out bool deleted))
                                    customerEntity.Deleted = deleted;
                                break;
                            case "company":
                                customerEntity.Company = value?.ToString();
                                break;
                                // CustomerId is handled elsewhere and not mapped here
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
                    string description = BuildCustomerDescription(customer, p);
                    _logger.LogError("Error while importing customer: {Description}. Error: {Error}", description, ex);
                    messages.Add($"&#x1F4CC; {description} could not be imported due to an error.");
                    failedCount++;
                }
            }

            string summary = $"Total Customers Processed: {totalCustomers}\n\nCreated: {createdCount}\nUpdated: {updatedCount}\nFailed: {failedCount}";
            string resultMessage = string.Join("\n", messages);

            var output = new ImportResultSummary
            {
                Summary = summary,
                Errors = resultMessage
            };

            return ServiceResult<ImportResultSummary>.Success(output);
        }
        private string BuildCustomerDescription(Dictionary<string, object> catData, ImportDataDto p)
        {
            List<string> parts = new List<string>();

            // Map system columns to tbl_Customer properties
            var customerIdKey = GetKey("CustomerId", p.ColumnMappingsList);
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

            var customerIdVal = !string.IsNullOrEmpty(customerIdKey) ? GetValue(catData, customerIdKey)?.ToString() : "";
            var fullNameVal = !string.IsNullOrEmpty(fullNameKey) ? GetValue(catData, fullNameKey)?.ToString() : "";
            var accountNumberVal = !string.IsNullOrEmpty(accountNumberKey) ? GetValue(catData, accountNumberKey)?.ToString() : "";
            var contactVal = !string.IsNullOrEmpty(contactKey) ? GetValue(catData, contactKey)?.ToString() : "";
            var cardNumberVal = !string.IsNullOrEmpty(cardNumberKey) ? GetValue(catData, cardNumberKey)?.ToString() : "";
            var vatNumberVal = !string.IsNullOrEmpty(vatNumberKey) ? GetValue(catData, vatNumberKey)?.ToString() : "";
            var emailVal = !string.IsNullOrEmpty(emailKey) ? GetValue(catData, emailKey)?.ToString() : "";
            var addressVal = !string.IsNullOrEmpty(addressKey) ? GetValue(catData, addressKey)?.ToString() : "";
            var creditLimitVal = !string.IsNullOrEmpty(creditLimitKey) ? GetValue(catData, creditLimitKey)?.ToString() : "";
            var deletedVal = !string.IsNullOrEmpty(deletedKey) ? GetValue(catData, deletedKey)?.ToString() : "";
            var companyVal = !string.IsNullOrEmpty(companyKey) ? GetValue(catData, companyKey)?.ToString() : "";

            if (!string.IsNullOrEmpty(customerIdVal))
                parts.Add($"CustomerId: {customerIdVal}");
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

            return "Customer [" + string.Join(", ", parts) + "]";
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

        #region Get Customers ForCSVExport BasedOn SelectedFields
        public async Task<ServiceResult<MemoryStream>> GetCustomersForCSVExportBySelectedFields(List<string> selectedColumnNames)
        {
            try
            {
                IQueryable<tbl_Customer> query = _context.tbl_Customers;

                // Build the dynamic SELECT clause
                var selectFields = new List<string>();
                var properties = typeof(tbl_Customer).GetProperties();
                foreach (var prop in properties)
                {
                    if (selectedColumnNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                        selectFields.Add(prop.Name);
                }

                // Project only the selected columns dynamically
                var dynamicQuery = query.Select($"new ({string.Join(", ", selectFields)})");

                var exportObject = dynamicQuery.Adapt<List<CustomersExportDto>>();
                //create excel file and return it
                var memorystream = await _excelDomainService.ExportExcelRecords(exportObject, selectedColumnNames, "Customers");

                return ServiceResult<MemoryStream>.Success(memorystream);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while exporting customers: {Error}", ex);
                return ServiceResult<MemoryStream>.Failure(
                    new ServerErrorException("Could not export customers."));
            }
        }
        #endregion

    }
}
