using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
    public class SupplierPaymentDAL : ISupplierPaymentDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<SupplierPaymentDAL> _logger;

        public SupplierPaymentDAL(ILogger<SupplierPaymentDAL> logger, mowtDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        #region Get Supplier Payment SUM from Database based on SupplierID and TransactionDate
        public async Task<ServiceResult<decimal>> GetSupplierPaymentSUMLowerThanEndDate(string SupplierID, DateTime EndDate)
        {
            try
            {
                var amount = await _context.tbl_SupplierPayments.Where(x => x.Amount > 0 && x.SupplierId == SupplierID && x.DateTimePayed <= EndDate)
                .SumAsync(x => x.Amount) ?? 0;

                return ServiceResult<decimal>.Success(amount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching supplier payment sum: {ex}", ex);
                return ServiceResult<decimal>.Failure(new ServerErrorException("Could not fetch supplier payment sum."));
            }
        }
        #endregion

        #region Get Supplier Debit SUM from Database based on SUpplier and TransactionDate
        public async Task<ServiceResult<decimal>> GetSupplierInvoiceSUMUsingSupplierIDAndEndDate(string SupplierID, DateTime EndDate)
        {
            try
            {
                var amount = await _context.tbl_ProductReceivings.Where(x => x.SupplierAccount == SupplierID && x.DateReceived <= EndDate && (x.CreditSupplierAcc ?? false))
                             .SumAsync(x => x.CostInc) ?? 0;

                return ServiceResult<decimal>.Success(amount);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching supplier invoice sum: {ex}", ex);
                return ServiceResult<decimal>.Failure(new ServerErrorException("Could not fetch supplier invoice sum."));
            }
        }
        #endregion

        #region ADD Supplier Direct Payment to Database

        public async Task<ServiceResult<SupplierPaymentDto>> AddSupplierPaymentToDB([Required] SupplierPaymentDto spDto)
        {
            try
            {
                var sp = spDto.Adapt<tbl_SupplierPayment>();
                await _context.tbl_SupplierPayments.AddAsync(sp);
                await _context.SaveChangesAsync();
                return ServiceResult<SupplierPaymentDto>.Success(sp.Adapt<SupplierPaymentDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while saving Supplier payment: {ex}", ex);
                return ServiceResult<SupplierPaymentDto>.Failure(
                    new ServerErrorException("Could not Add supplier Payment"));
            }

        }
        #endregion
    }
}
