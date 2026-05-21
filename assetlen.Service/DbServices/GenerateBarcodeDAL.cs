using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.statics;
using assetlen.Shared.Models.ViewModels;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace assetlen.Service.DbServices
{
    public class GenerateBarcodeDAL : IGenerateBarcodeDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<GenerateBarcodeDAL> _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IConfigDAL _configDAL;

        public GenerateBarcodeDAL(ILogger<GenerateBarcodeDAL> logger, mowtDbContext context, IConfigDAL configDAL)
        {
            _logger = logger;
            _context = context;
            _configDAL = configDAL;
        }

        private string AddLastChecksumDigitForBarcode(string twelveDigitCode)
        {
            var sum = 0;

            for (var i = twelveDigitCode.Length; i >= 1; i--)
            {
                var d = Convert.ToInt32(twelveDigitCode.Substring(i - 1, 1));
                var f = i % 2 == 0 ? 3 : 1;
                sum += d * f;
            }
            var checksum = (10 - (sum % 10)) % 10;

            return twelveDigitCode + checksum;
        }
        private ServiceResult<string> GenerateUnique12digitBarcodestring(string fiveDigitPrefix, int uniqueNumber)
        {

            string barcode12Output = "";
            string barcode7Output = "";

            using (TransactionScope Scope = new TransactionScope())
            {
                if (uniqueNumber < 10000000)
                {
                    if (uniqueNumber > 0)
                    {
                        int zerosToAdd = 7 - uniqueNumber.ToString().Length;

                        for (int i = 0; i < zerosToAdd; i++)
                        {
                            barcode7Output += "0";
                        }

                        barcode7Output += uniqueNumber.ToString();
                    }
                    if (barcode7Output.Length == 7)
                    {
                        barcode12Output = fiveDigitPrefix + barcode7Output;
                    }
                }
                else
                {
                    ServiceResult<string>.Failure(new BadRequestException("There was error in Generating the barcode. Your system has generated more  than the 10M bar codes possible for the EAN13 Barcode system. contact mowt Support if you still need more Barcodes"));

                }
            }

            return ServiceResult<string>.Success(barcode12Output);
        }
        #region GenerateNextBarcode
        public async Task<ServiceResult<string>> GenerateNextBarcode([MinLength(5)] string companyCode = "")
        {
            try
            {
                var configResult = await _configDAL.GetSettingByID((int)statics.Configurations.BarcodePrefix);
                var currentPrefix = configResult?.Data?.StringValue ?? "59800";
                if (!string.IsNullOrEmpty(companyCode) && companyCode != currentPrefix)
                {
                    var configDto = new ConfigurationDto
                    {
                        ConfigId = (int)statics.Configurations.BarcodePrefix,
                        StringValue = companyCode
                    };
                    var prefixResult = await _configDAL.UpdateSettingInDB(configDto);
                    if (prefixResult.IsSuccess)
                    {
                        currentPrefix = prefixResult.Data.StringValue ?? currentPrefix;
                    }
                    else
                    {
                        currentPrefix = companyCode;
                    }
                }
                else if (string.IsNullOrEmpty(companyCode))
                {
                    companyCode = currentPrefix;
                }
                var uniqueNo = await GetUniqueBarcodeNumberFromDB();

                if (uniqueNo.IsSuccess)
                {
                    var result12 = GenerateUnique12digitBarcodestring(companyCode, (int)(uniqueNo.Data.Number ?? 1));

                    if (result12.IsSuccess)
                    {
                        var output13 = AddLastChecksumDigitForBarcode(result12.Data);
                        return ServiceResult<string>.Success(output13);
                    }
                    return result12;
                }
                return ServiceResult<string>.Failure(uniqueNo.Error);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while generating next barcode: {Error}", ex);
                return ServiceResult<string>.Failure(new ServerErrorException("Could not generate next barcode."));
            }
        }
        #endregion

        #region Get Barcodes
        public async Task<ServiceResult<List<string>>> GenerateBarcodes(int n, string companyCode = "")
        {
            if (n <= 0)
            {
                return ServiceResult<List<string>>.Failure(new BadRequestException("The count must be greater than zero."));
            }

            var barcodes = new List<string>();

            for (int i = 0; i < n; i++)
            {
                var barcodeResult = await GenerateNextBarcode(companyCode);
                if (!barcodeResult.IsSuccess)
                {
                    return ServiceResult<List<string>>.Failure(barcodeResult.Error);
                }
                barcodes.Add(barcodeResult.Data);
            }

            return ServiceResult<List<string>>.Success(barcodes);
        }

        #endregion

        #region Create  UniqueBarcodeNumber
        public async Task<ServiceResult<UniqueFieldDto>> CreateBarcodeNumberInDB(UniqueFieldDto uniqueFieldDto)
        {
            if (uniqueFieldDto == null) return ServiceResult<UniqueFieldDto>.Failure(
                                            new BadRequestException("Bar code data is required."));
            try
            {
                var uniqueField = uniqueFieldDto.Adapt<tbl_UniqueField>();

                _context.tbl_UniqueFields.Add(uniqueField);
                await _context.SaveChangesAsync();

                var createdUniqueFieldDto = uniqueField.Adapt<UniqueFieldDto>();

                return ServiceResult<UniqueFieldDto>.Success(createdUniqueFieldDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating unique barcode number: {Error}", ex);
                return ServiceResult<UniqueFieldDto>.Failure(
                    new ServerErrorException("Could not create unique barcode number."));
            }
        }
        #endregion

        #region Update  UniqueBarcodeNumber
        public async Task<ServiceResult<UniqueFieldDto>> UpdateBarcodeNumberInDB(UniqueFieldDto uniqueFieldDto)
        {
            if (uniqueFieldDto == null)
                return ServiceResult<UniqueFieldDto>.Failure(new BadRequestException("Bar code data is required."));

            try
            {
                var uniqueField = await _context.tbl_UniqueFields
                    .FirstOrDefaultAsync(uf => uf.UniqueField == "Barcode");

                if (uniqueField == null)
                    return ServiceResult<UniqueFieldDto>.Failure(new NotFoundException("Unique field not found."));

                uniqueField.Number = uniqueFieldDto.Number;

                _context.tbl_UniqueFields.Update(uniqueField);
                await _context.SaveChangesAsync();

                var updatedUniqueFieldDto = uniqueField.Adapt<UniqueFieldDto>();

                return ServiceResult<UniqueFieldDto>.Success(updatedUniqueFieldDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating unique barcode number: {Error}", ex);
                return ServiceResult<UniqueFieldDto>.Failure(
                    new ServerErrorException("Could not update unique barcode number."));
            }
        }
        #endregion

        #region Read Barcode Unique Number from Database
        public async Task<ServiceResult<UniqueFieldDto>> GetUniqueBarcodeNumberFromDB()
        {
            await _semaphore.WaitAsync();

            try
            {
                var uniqueField = _context.tbl_UniqueFields
                                        .Where(uf => uf.UniqueField == "Barcode")
                                        .FirstOrDefault();

                if (uniqueField == null)
                {

                    var uniqueField2 = new UniqueFieldDto()
                    {
                        Number = 1,
                        UniqueField = "Barcode"
                    };

                    var created = await CreateBarcodeNumberInDB(uniqueField2);
                    return ServiceResult<UniqueFieldDto>.Success(created.Data.Adapt<UniqueFieldDto>());

                }
                uniqueField.Number += 1;
                var unique = uniqueField.Adapt<UniqueFieldDto>();
                var updateResult = await UpdateBarcodeNumberInDB(unique);

                if (!updateResult.IsSuccess)
                {
                    return ServiceResult<UniqueFieldDto>.Failure(updateResult.Error);
                }

                return ServiceResult<UniqueFieldDto>.Success(updateResult.Data.Adapt<UniqueFieldDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching unique barcode number: {Error}", ex);
                return ServiceResult<UniqueFieldDto>.Failure(
                    new ServerErrorException("Could not fetch unique barcode number."));
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

    }
}
