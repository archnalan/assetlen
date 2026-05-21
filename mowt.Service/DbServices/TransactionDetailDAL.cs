using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Service.Extensions;
using mowt.Service.FileProcessingServices;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ExportDtos;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using Mapster;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mowt.Shared.Models.Models;
using Google.Apis.Util;

namespace mowt.Service.DbServices
{
    public class TransactionDetailDAL : ITransactionDetailDAL
    {
        private readonly mowtDbContext _context;

        private readonly ILogger<TransactionDetailDAL> _logger;
        private readonly IProductsDAL _productsDAL;
        private readonly IPricingCalculations _pricingCalculations;
        private readonly ItaxDAL _taxDAL;
        private readonly IExcelDomainService _excelDomainService;

        public TransactionDetailDAL(ILogger<TransactionDetailDAL> logger, mowtDbContext context, IProductsDAL productsDAL, IPricingCalculations pricingCalculations, ItaxDAL taxDAL, IExcelDomainService excelDomainService)
        {
            _logger = logger;
            _context = context;
            _productsDAL = productsDAL;
            _pricingCalculations = pricingCalculations;
            _taxDAL = taxDAL;
            _excelDomainService = excelDomainService;
        }


        #region Create New TransactionDetail

        public async Task<ServiceResult<List<TransactionDetailDto>>> CreateOrSyncNewTransactionDetails(List<TransactionDetailDto> tdDto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        List<tbl_TransactionDetail> output = new List<tbl_TransactionDetail>();
                        var itemsOnTransaction = _context.tbl_TransactionDetails.AsNoTracking().Where(x => x.TransactionId == tdDto.First().TransactionId).ToList();

                        var validatedResult = await ReCalculatePrices(tdDto);
                        if (!validatedResult.IsSuccess)
                            ServiceResult<List<TransactionDetailDto>>.Failure(validatedResult.Error);

                        var td = tdDto.Adapt<List<tbl_TransactionDetail>>();
                        if (itemsOnTransaction.Count() == 0)
                        {
                            //first time syncing cart
                            td.ForEach(x =>
                            {
                                x.Id = Guid.NewGuid().ToString();
                                x.Tax = null;
                                x.Product = null;
                                x.Discount = null;
                                x.DiscountId = string.IsNullOrEmpty(x.DiscountId) ? null : x.DiscountId;
                            });
                            await _context.tbl_TransactionDetails.AddRangeAsync(td);
                            output.AddRange(td);
                            await _context.SaveChangesAsync();

                        }
                        else
                        {
                            //items that already exist in the db
                            foreach (var item in td)
                            {
                                item.Tax = null;
                                item.Product = null;
                                item.Discount = null;
                                item.DiscountId = string.IsNullOrEmpty(item.DiscountId) ? null : item.DiscountId;
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    item.Id = Guid.NewGuid().ToString();
                                    await _context.tbl_TransactionDetails.AddAsync(item);
                                    output.Add(item);
                                }
                                else
                                {
                                    var existingEntry = _context.tbl_TransactionDetails
                                        .FirstOrDefault(e => e.Id == item.Id);

                                    if (existingEntry == null)
                                    {
                                        await transaction.RollbackAsync();
                                        return ServiceResult<List<TransactionDetailDto>>.Failure(new NotFoundException($"Error while updating transaction detail. Item id {item.Id} does not exist."));

                                    }
                                    existingEntry.Qty = item.Qty;
                                    existingEntry.SortOrder = item.SortOrder;
                                    existingEntry.DiscountId = item.DiscountId;
                                    existingEntry.DiscountPercent = item.DiscountPercent;
                                    existingEntry.CostExc = item.CostExc;
                                    existingEntry.CostInc = item.CostExc;
                                    existingEntry.PriceExc = item.PriceExc;
                                    existingEntry.PriceInc = item.PriceInc;
                                    existingEntry.TotalPriceInc = item.TotalPriceInc;
                                    existingEntry.TotalPriceExc = item.TotalPriceExc;
                                    existingEntry.ItemNote = item.ItemNote;
                                    existingEntry.SpecialPricingUsed = item.SpecialPricingUsed;


                                    output.Add(existingEntry);

                                }

                            }
                            //if it was deleted
                            var incomingOldIds = td.Where(x => !string.IsNullOrEmpty(x.Id)).Select(x => x.Id);

                            var deletedItems = itemsOnTransaction.Where(x => !(incomingOldIds.Contains(x.Id))).ToList();

                            if (deletedItems.Count() > 0) _context.tbl_TransactionDetails.RemoveRange(deletedItems);
                            //update related sale total
                            var sale = await _context.tbl_Transactions.FirstOrDefaultAsync(x => x.Id == td.First().TransactionId);
                            if (sale != null)
                            {
                                sale.SaleTotal = output.Sum(x => x.TotalPriceInc);
                                sale.TransactionDate = DateTime.UtcNow; // last updated
                            }
                            await _context.SaveChangesAsync();

                        }

                        await transaction.CommitAsync();

                        var createdDto = output.Adapt<List<TransactionDetailDto>>();

                        return ServiceResult<List<TransactionDetailDto>>.Success(createdDto);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Error while creating transaction detail: {error}", ex);
                        return ServiceResult<List<TransactionDetailDto>>.Failure(new ServerErrorException($"Error while creating transaction detail: {ex.Message}"));
                    }
                }
            });

        }

        private async Task<ServiceResult<List<TransactionDetailDto>>> ReCalculatePrices(List<TransactionDetailDto> tdDto)
        {
            try
            {
                var detailItems = new List<TransactionDetailDto>();
                foreach (var transItem in tdDto)
                {
                    var productResult = await _productsDAL.GetProductsBasedOnID(transItem.ProductId ?? "");
                    if (!productResult.IsSuccess)
                    {
                        return ServiceResult<List<TransactionDetailDto>>.Failure(productResult.Error);
                    }
                    var productPricing = productResult.Data.Adapt<ProductPricing>();
                    if (!string.IsNullOrEmpty(transItem.TaxId))
                    {
                        var taxResult = await _taxDAL.GetTaxFromDBbasedOnTaxID(transItem.TaxId);
                        if (taxResult.IsSuccess)
                        {
                            transItem.Tax = taxResult.Data;
                        }
                        else
                        {
                            return ServiceResult<List<TransactionDetailDto>>.Failure(taxResult.Error);
                        }
                    }
                    var validationResult = _pricingCalculations.FullCalculationsCheck(transItem, productPricing, true);//True will override calculations while False returns errors if any
                    if (!validationResult.IsSuccess)
                    {
                        return ServiceResult<List<TransactionDetailDto>>.Failure(validationResult.Error);
                    }
                    validationResult.Data.Adapt(transItem);
                    detailItems.Add(transItem);
                }
                return ServiceResult<List<TransactionDetailDto>>.Success(detailItems);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while recalculating prices: {error}", ex);
                return ServiceResult<List<TransactionDetailDto>>.Failure(new ServerErrorException("Error while recalculating prices."));
            }

        }
        #endregion

        #region update transactionDetail in the  DB

        public async Task<ServiceResult<TransactionDetailDto>> UpdateTransactionDetail(TransactionDetailDto tdDto)
        {
            try
            {
                var td = tdDto.Adapt<tbl_TransactionDetail>();

                td.ProductId = tdDto.ProductId ?? td.ProductId;

                string sql = "UPDATE  dbo.tbl_transactionDetail SET ";
                int sqlLength1 = sql.Length;
                int sqlLength2 = sql.Length;

                bool x()
                {
                    return sql.Length > sqlLength1;
                }

                if (!(td.ProductId == default(string)))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }

                    sql = sql + " productID=@productID";

                }
                if (td.Qty != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " qty=@qty ";
                }
                if (td.CostExc != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " costExc=@costExc ";
                }

                if (td.CostInc != default(decimal))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " costInc=@costInc";
                }
                if (td.PriceInc != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " priceInc=@priceInc";
                }
                if (td.PriceExc != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " priceExc=@priceExc";
                }
                if (td.TaxId != default(string))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " taxID=@taxID ";
                }
                if (td.TaxPercent != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " taxPercent=@taxPercent";
                }
                if (td.DiscountId != default(string))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " discountID=@discountID";
                }

                if (td.DiscountPercent != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " discountPercent=@discountPercent";
                }
                //else if (td.discountPercent == default(int) && statics.frmDiscount != null)
                //{
                //	frmDiscount frm = (frmDiscount)statics.frmDiscount;
                //	if (frm.UpdateZeros)
                //	{
                //		if (x())
                //		{
                //			sql = sql + " ,";
                //			sqlLength1 = sql.Length;
                //		}
                //		sql = sql + " discountPercent=@discountPercent";
                //	}
                //	statics.frmDiscount = null;
                //}

                if (td.TransactionId != default(string))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " transactionID=@transactionID";
                }
                if (td.TotalPriceExc != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " totalPriceExc=@totalPriceExc";
                }
                else if (td.Qty != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " totalPriceExc=@qty*priceExc";
                }
                if (td.TotalPriceInc != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " totalPriceInc=@totalPriceInc";
                }
                if (td.SortOrder != default(int))
                {
                    if (x())
                    {
                        sql = sql + " ,";
                        sqlLength1 = sql.Length;
                    }
                    sql = sql + " sortOrder=@sortOrder";
                }

                if (x())
                {
                    sql = sql + " ,";
                    sqlLength1 = sql.Length;
                }
                sql = sql + " specialPricingUsed=@specialPricingUsed";

                //if (sqlLength2 == sql.Length)
                //{
                //    sql = "UPDATE  dbo.tbl_transactionDetail SET totalPriceInc=@totalPriceInc";
                //}


                sql = sql + "  WHERE detailID=@detailID; SELECT @@IDENTITY";

                var parameters = new[]
                {
                    new SqlParameter("@productID", td.ProductId),
                    new SqlParameter("@qty", td.Qty),
                    new SqlParameter("@costExc", td.CostExc),
                    new SqlParameter("@costInc", td.CostInc),
                    new SqlParameter("@priceExc", td.PriceExc),
                    new SqlParameter("@taxID", td.TaxId),
                    new SqlParameter("@taxPercent", td.TaxPercent),
                    new SqlParameter("@discountID", td.DiscountId),
                    new SqlParameter("@discountPercent", td.DiscountPercent),
                    new SqlParameter("@transactionID", td.TransactionId),
                    new SqlParameter("@detailID", td.Id),
                    new SqlParameter("@totalPriceInc", td.TotalPriceInc),
                    new SqlParameter("@totalPriceExc", td.TotalPriceExc),
                    new SqlParameter("@sortOrder", td.SortOrder),
                    new SqlParameter("@specialPricingUsed", td.SpecialPricingUsed),
                };

                int rowsUpdated = await _context.Database.SqlQueryRaw<int>(sql, parameters).FirstOrDefaultAsync();
                //int rowsUpdated = await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                if (rowsUpdated == 0)
                {
                    _logger.LogError($"Transaction detail with iD: {tdDto.Id} not found nor updated.");
                    return ServiceResult<TransactionDetailDto>.Failure(
                        new ServerErrorException($"Transaction detail with iD: {tdDto.Id} not found nor updated."));
                }

                var updatedDetail = await _context.tbl_TransactionDetails.FindAsync(tdDto.Id);

                return ServiceResult<TransactionDetailDto>.Success(updatedDetail.Adapt<TransactionDetailDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating transaction detail: {error}", ex);
                return ServiceResult<TransactionDetailDto>.Failure(
                    new ServerErrorException("Error while updating transaction detail."));
            }
        }
        #endregion

        #region Read transaction from Database based on transactionID
        public async Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionID(string transId, bool? completed, string? statusOrder, int? saleStatus)
        {
            try
            {
                IQueryable<tbl_Transaction> query = _context.tbl_Transactions;

                if (completed == true)
                {
                    query = query.Where(x => x.TransactionStatus > 9 && x.TransactionStatus <= 20);
                }
                else if (completed == false)
                {
                    query = query.Where(x => x.TransactionStatus < 10);
                }

                if (saleStatus.HasValue)
                {
                    query = query.Where(x => x.TransactionStatus == saleStatus.Value);
                }

                if (!string.IsNullOrEmpty(statusOrder))
                {
                    query = query.Where(x => x.OrderStatus == statusOrder);
                }

                var transaction = query.FirstOrDefault(x => x.Id == transId);

                if (transaction == null)
                {
                    _logger.LogError("Transaction with ID: {transId} not found.", transId);
                    return ServiceResult<List<TransactionDetailDto>>.Failure(
                        new NotFoundException($"Transaction with ID: {transId} not found."));
                }

                var transDetail = await _context.tbl_TransactionDetails
                    .Where(x => x.TransactionId == transId)
                    .Include(x => x.Tax)
                    .Include(x => x.Product)
                    .ToListAsync();

                var saleDto = transDetail.Adapt<List<TransactionDetailDto>>();

                return ServiceResult<List<TransactionDetailDto>>.Success(saleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching transaction detail: {error}", ex);
                return ServiceResult<List<TransactionDetailDto>>.Failure(
                    new ServerErrorException($"Error while fetching transaction detail: {ex.Message}"));
            }
        }
        #endregion

        #region Read Transaction details with transaction details from Database
        public async Task<ServiceResult<TransactionDetailDto>> GetTransactionDetailWithRelatedDataFromDB(string detailId)
        {
            try
            {
                //cartesian explosion issue likely
                var Detail = await _context.tbl_TransactionDetails
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .Include(y => y.Tax)
                    .Include(x => x.Discount)
                    //.Include(x => x.Transaction)
                    .Where(x => x.Id == detailId)
                    .SingleOrDefaultAsync();

                if (Detail == null)
                {
                    _logger.LogError($"Transaction Detail with ID: {detailId} not found.");
                    return ServiceResult<TransactionDetailDto>.Failure(
                        new NotFoundException($"Transaction Detail with ID: {detailId} not found."));
                }
                var transactionDetailDto = GetTransactionDetailDto(Detail);

                return ServiceResult<TransactionDetailDto>.Success(transactionDetailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching transaction detail with ID {id} in db{error}.", detailId, ex);

                return ServiceResult<TransactionDetailDto>.Failure(
                    new ServerErrorException("Could not fetch Transaction detail."));
            }
        }
        #endregion

        #region Read transaction from Database based on transactionID and sortID
        public async Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionIDandSortID(string transId, int sortOrder)
        {
            try
            {

                var transDetail = await _context.tbl_TransactionDetails.Where(x => x.TransactionId == transId
                && x.SortOrder == sortOrder).ToListAsync();

                return ServiceResult<List<TransactionDetailDto>>.Success(transDetail.Adapt<List<TransactionDetailDto>>());

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating transaction detail: {error}", ex);
                return ServiceResult<List<TransactionDetailDto>>.Failure(
                    new ServerErrorException("Could not fetch Transaction detail."));
            }
        }
        #endregion

        #region Read transaction from Database based on transactionID and special pricing
        public async Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionIDandSpecialPricing(string transId, bool spPricing)
        {
            try
            {
                var transDetail = await _context.tbl_TransactionDetails.Where(x => x.TransactionId == transId
                && x.SpecialPricingUsed == spPricing).ToListAsync();

                return ServiceResult<List<TransactionDetailDto>>.Success(transDetail.Adapt<List<TransactionDetailDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching transaction detail: {error}", ex);
                return ServiceResult<List<TransactionDetailDto>>.Failure(
                    new ServerErrorException("Could not fetch Transaction detail."));
            }
        }
        #endregion

        #region Read transaction from Database based on transactionID and productID
        public async Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionDetailBasedOnTransactionIDandProdID(string transId, string prodId)
        {
            try
            {
                var transDetail = await _context.tbl_TransactionDetails.Where(x => x.TransactionId == transId && x.ProductId == prodId).ToListAsync();

                return ServiceResult<List<TransactionDetailDto>>.Success(transDetail.Adapt<List<TransactionDetailDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching transaction details: {error}", ex);
                return ServiceResult<List<TransactionDetailDto>>.Failure(
                    new ServerErrorException("Could not fetch Transaction detail."));
            }
        }
        #endregion

        #region Read transaction TotalInc from Database based on transactionID
        public async Task<ServiceResult<decimal>> GetTransactionTotalInc(string transId)
        {
            try
            {
                string sql = "select COALESCE(SUM(totalPriceInc),0) from tbl_transactionDetail where transactionId = @transactionId;";
                var parameter = new SqlParameter("@transactionId", transId);

                decimal sum = await _context.Database.SqlQueryRaw<decimal>(sql, parameter).FirstOrDefaultAsync();
                //decimal sum = await _context.Database.ExecuteSqlRawAsync(sql, parameter);

                return ServiceResult<decimal>.Success(sum);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching transaction total inc in db {error}", ex);
                return ServiceResult<decimal>.Failure(
                    new ServerErrorException("Could not fetch Transaction detail."));
            }
        }
        #endregion

        #region Read transaction TotalExc from Database based on transactionID
        public async Task<ServiceResult<decimal>> GetTransactionTotalExc(string transId)
        {
            try
            {
                string sql = "select COALESCE(SUM(totalPriceExc),0) from tbl_transactionDetail where transactionId = @transactionId;";
                var parameter = new SqlParameter("@transactionId", transId);

                decimal sum = await _context.Database.SqlQueryRaw<decimal>(sql, parameter).FirstOrDefaultAsync();
                //decimal sum = await _context.Database.ExecuteSqlRawAsync(sql, parameter);

                return ServiceResult<decimal>.Success(sum);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching transaction total exc in db {error}", ex);
                return ServiceResult<decimal>.Failure(
                    new ServerErrorException($"Could not fetch Transaction total price exclusive."));
            }
        }
        #endregion

        #region Delete all transaction details based on transactionID

        public async Task<ServiceResult<bool>> DeleteTransactionDetailBasedOnTransactionID(string transId)
        {
            var detailsInDb = await _context.tbl_TransactionDetails.Where(x => x.TransactionId == transId).ToListAsync();

            if (detailsInDb == null) return ServiceResult<bool>
                    .Failure(new NotFoundException($"Transaction details with transaction ID: {transId} not found."));

            try
            {
                //change delete property
                foreach (var detail in detailsInDb)
                {
                    detail.IsDeleted = true;
                }

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Transaction details with transaction ID:{transactionId} could not be deleted.{Error}", transId, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException($"Transaction details with transaction ID:{transId} could not be deleted"));
            }
        }
        #endregion

        #region Delete Selected transaction details based on detailID

        public async Task<ServiceResult<bool>> DeleteTransactionDetailPerDetailID(string detailID)
        {
            var detailInDb = await _context.tbl_TransactionDetails.FindAsync(detailID);

            if (detailInDb == null) return ServiceResult<bool>
                    .Failure(new NotFoundException($"Transaction with ID: {detailID} not found."));

            try
            {
                //change delete property
                detailInDb.IsDeleted = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Transaction detail with ID {transactionId} could not be deleted.{Error}", detailID, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException($"Transaction detail with ID: {detailID} could not be deleted"));
            }
        }
        #endregion

        #region Read transactionDetail from Database based on transactionDetailID
        public async Task<ServiceResult<TransactionDetailDto>> GetTransactionDetailBasedOnDetailID(string detailID)
        {
            try
            {
                var Transaction = await _context.tbl_TransactionDetails.Include(x => x.Tax).FirstOrDefaultAsync(x => x.Id == detailID);

                if (Transaction == null)
                {
                    _logger.LogError($"Transaction detail with ID: {detailID} not found.");
                    return ServiceResult<TransactionDetailDto>.Failure(
                        new NotFoundException($"Transaction detail with ID: {detailID} not found."));
                }

                return ServiceResult<TransactionDetailDto>.Success(Transaction.Adapt<TransactionDetailDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching transaction detail with ID {detailID} in deb{error}", detailID, ex);

                return ServiceResult<TransactionDetailDto>.Failure(
                    new ServerErrorException($"Error fetching transaction detail: {ex.Message}"));
            }
        }
        #endregion

        #region get a transaction detail dto from the database's transaction detail
        public TransactionDetailDto GetTransactionDetailDto(tbl_TransactionDetail detail)
        {
            var transactionDetailDto = new TransactionDetailDto
            {
                Id = detail.Id,
                ProductId = detail.ProductId,
                Qty = detail.Qty,
                CostExc = detail.CostExc,
                CostInc = detail.CostInc,
                PriceInc = detail.PriceInc,
                PriceExc = detail.PriceExc,
                TaxId = detail.TaxId,
                TaxPercent = detail.TaxPercent,
                DiscountId = detail.DiscountId,
                DiscountPercent = detail.DiscountPercent,
                TransactionId = detail.TransactionId,
                TotalPriceInc = detail.TotalPriceInc,
                TotalPriceExc = detail.TotalPriceExc,
                SortOrder = detail.SortOrder,
                CostIncState = detail.CostIncState,
                SpecialPricingUsed = detail.SpecialPricingUsed,

                // Prevent recursion by not mapping TransactionDto fully
                // Excluding: detail.Transaction

                // Map Discount if available
                Discount = detail.Discount != null ? new DiscountDto
                {
                    Id = detail.Discount.Id,
                    DiscountName = detail.Discount.DiscountName,
                    DiscountValue = detail.Discount.DiscountValue,
                    isValuePercentage = detail.Discount.isValuePercentage,
                    Active = detail.Discount.Active
                } : null,

                // Map Tax if available
                Tax = detail.Tax != null ? new taxDto
                {
                    Id = detail.Tax.Id,
                    TaxValue = detail.Tax.TaxValue,
                    TaxDescription = detail.Tax.TaxDescription,
                } : null,

                // Map Product if available
                Product = detail.Product != null ? new ProductsDto
                {
                    Id = detail.Product.Id,
                    ProductCode = detail.Product.ProductCode,
                    BarCode = detail.Product.BarCode,
                    ProductName = detail.Product.ProductName,
                    CostExclusive = detail.Product.CostExclusive,
                    CostInclusive = detail.Product.CostInclusive,
                    InStock = detail.Product.InStock,
                    PriceExclusive = detail.Product.PriceExclusive,
                    PriceExclusive2 = detail.Product.PriceExclusive2,
                    PriceInclusive = detail.Product.PriceInclusive,
                    PriceInclusive2 = detail.Product.PriceInclusive2,
                    CategoryId = detail.Product.CategoryId,
                    Location = detail.Product.Location,
                    SegmentId = detail.Product.SegmentId,
                    SupplierId = detail.Product.SupplierId,
                    ProductImage = detail.Product.ProductImage,
                    //CreatedBy = detail.Product.CreatedBy,
                    Deleted = detail.Product.Deleted,
                    TrackInventory = detail.Product.TrackInventory,
                    ReOrderLevel = detail.Product.ReOrderLevel,
                    ReOrderQty = detail.Product.ReOrderQty,
                    //Favourite = detail.Product.Favourite,
                    HasSubProduct = detail.Product.HasSubProduct,
                    IsAsubProduct = detail.Product.IsAsubProduct,
                    CompoundCostPricing = detail.Product.CompoundCostPricing,
                    TaxId = detail.Product.TaxId,
                    CostIncStatus = detail.Product.CostIncStatus,

                    // Prevent recursion in ProductsDto
                    Tax = detail.Product.Tax != null ? new taxDto
                    {
                        Id = detail.Product.Tax.Id,
                        TaxValue = detail.Product.Tax.TaxValue,
                        TaxDescription = detail.Product.Tax.TaxDescription,
                    } : null
                } : null
            };

            return transactionDetailDto;
        }
        public async Task<ServiceResult<MemoryStream>> GetTransactionDetailsForCSVExportBySelectedFields(List<string> selectedColumnNames)
        {
            try
            {
                IQueryable<tbl_TransactionDetail> query = _context.tbl_TransactionDetails;



                // Fetch related data for names  
                var transactionDetailsWithRelatedData = await query
                    .Select(x => new
                    {
                        x.Id,
                        x.ProductId,
                        x.TaxId,
                        x.TaxPercent,
                        x.TransactionId,
                        x.Qty,
                        x.CostExc,
                        x.CostInc,
                        x.PriceExc,
                        x.PriceInc,
                        x.TotalPriceExc,
                        x.TotalPriceInc,
                        x.SortOrder,
                        x.SpecialPricingUsed,
                        x.DiscountPercent,
                        ProductName = x.ProductId != null ? _context.tbl_Products.FirstOrDefault(p => p.Id == x.ProductId).ProductName : null,
                        TaxName = x.TaxId != null ? _context.tbl_Taxes.FirstOrDefault(t => t.Id == x.TaxId).TaxDescription : null
                    })
                    .ToListAsync();

                // Map to export DTO and filter selected fields  
                var exportObject = transactionDetailsWithRelatedData.Select(detail =>
                {
                    var exportDto = new TransactionDetailExportDto();
                    foreach (var field in selectedColumnNames)
                    {
                        switch (field.ToLower())
                        {
                            case "detailid":
                                exportDto.DetailId = detail.Id;
                                break;
                            case "productname":
                                exportDto.ProductName = detail.ProductName;
                                break;
                            case "taxname":
                                exportDto.TaxName = detail.TaxName;
                                break;
                            case "taxpercent":
                                exportDto.TaxPercent = detail.TaxPercent;
                                break;
                            case "transactionid":
                                exportDto.TransactionId = detail.TransactionId;
                                break;
                            case "qty":
                                exportDto.Qty = detail.Qty;
                                break;
                            case "costexc":
                                exportDto.CostExc = detail.CostExc;
                                break;
                            case "costinc":
                                exportDto.CostInc = detail.CostInc;
                                break;
                            case "priceexc":
                                exportDto.PriceExc = detail.PriceExc;
                                break;
                            case "priceinc":
                                exportDto.PriceInc = detail.PriceInc;
                                break;
                            case "totalpriceexc":
                                exportDto.TotalPriceExc = detail.TotalPriceExc;
                                break;
                            case "totalpriceinc":
                                exportDto.TotalPriceInc = detail.TotalPriceInc;
                                break;
                            case "sortorder":
                                exportDto.SortOrder = detail.SortOrder;
                                break;
                            case "discountpercent":
                                exportDto.DiscountPercent = detail.DiscountPercent;
                                break;
                        }
                    }
                    return exportDto;
                }).ToList();

                // Create Excel file and return it  
                var memoryStream = await _excelDomainService.ExportExcelRecords(exportObject, selectedColumnNames, "Transaction Details");
                return ServiceResult<MemoryStream>.Success(memoryStream);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while exporting transaction details: {ex}", ex);
                return ServiceResult<MemoryStream>.Failure(new ServerErrorException("Could not export transaction details."));
            }
        }
        public async Task<ServiceResult<ImportResultSummary>> ImportTransactionDetailsFromExcel(ImportDataDto p)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                using (var scope = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (p == null)
                            return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("Import data is required."));

                        int totalTransactionDetails = 0;
                        int createdCount = 0;
                        int failedCount = 0;
                        List<string> messages = new List<string>();

                        if (p.UploadedExcelContent == null || p.UploadedExcelContent.Count == 0)
                        {
                            return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No data found in the uploaded file."));
                        }
                        if (p.ColumnMappingsList == null || p.ColumnMappingsList.Count == 0)
                        {
                            return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No column mappings provided."));
                        }

                        // Group the data by TransactionId  
                        var groupedData = p.UploadedExcelContent.GroupBy(detail =>
                        {
                            var transactionIdKey = GetKey("TransactionId", p.ColumnMappingsList);
                            return !string.IsNullOrEmpty(transactionIdKey) ? GetValue(detail, transactionIdKey)?.ToString() : string.Empty;
                        });

                        foreach (var group in groupedData)
                        {
                            string transactionIdStr = group.Key;
                            if (string.IsNullOrEmpty(transactionIdStr))
                            {
                                messages.Add($"Transaction group with missing TransactionId was skipped.");
                                failedCount += group.Count();
                                continue;
                            }
                            var transactDateKey = GetKey("TransactionDate", p.ColumnMappingsList);
                            string detailDateStr = !string.IsNullOrEmpty(transactDateKey) ? GetValue(group.FirstOrDefault(), transactDateKey)?.ToString()! : string.Empty;
                            if (!DateTime.TryParse(detailDateStr, out DateTime date))
                            {
                                messages.Add($"Transaction group with transaction Id {group.Key} has missing TransactionDate and was skipped.");
                                failedCount += group.Count();
                                continue;
                            }
                            var soldByKey = GetKey("SoldBy", p.ColumnMappingsList);
                            string soldByStr = !string.IsNullOrEmpty(soldByKey) ? GetValue(group.FirstOrDefault(), soldByKey)?.ToString()! : string.Empty;
                            var soldBy = _context.Users.FirstOrDefault(x => x.LastName.Contains(soldByStr) || x.FirstName.Contains(soldByStr) || x.UserName.Contains(soldByStr));

                            if (soldBy == null)
                            {
                                var random = Guid.NewGuid().ToString().Substring(0, 5);
                                soldBy = new AppUser()
                                {
                                    FirstName = soldByStr.Split(' ').FirstOrDefault()?.Trim() ?? random,
                                    LastName = soldByStr.Split(' ').LastOrDefault()?.Trim() ?? random,
                                    UserName = $"{soldByStr.Replace(" ", "").ToLowerInvariant().Trim()}_{random}",
                                    Email = $"{soldByStr.Replace(" ", "").ToLowerInvariant().Trim()}_{random}@mowtdefault.com",
                                };
                                await _context.Users.AddAsync(soldBy);
                                await _context.SaveChangesAsync();
                            }

                            var customerKey = GetKey("CustomerName", p.ColumnMappingsList);
                            string customerStr = !string.IsNullOrEmpty(customerKey) ? GetValue(group.FirstOrDefault(), customerKey)?.ToString()! : string.Empty;
                            var customer = _context.tbl_Customers.FirstOrDefault(x => x.FullName == customerStr);

                            if (customer == null)
                            {
                                customer = new tbl_Customer()
                                {
                                    FullName = customerStr,
                                };
                                await _context.tbl_Customers.AddAsync(customer);
                                await _context.SaveChangesAsync();
                            }
                            if (!DateTime.TryParse(detailDateStr, out DateTime dateOut))
                            {
                                messages.Add($"Transaction group with transaction Id {group.Key} has missing TransactionDate and was skipped.");
                                failedCount += group.Count();
                                continue;
                            }
                            var commentKey = GetKey("TransactionComment", p.ColumnMappingsList);
                            string commentStr = !string.IsNullOrEmpty(customerKey) ? GetValue(group.FirstOrDefault(), customerKey)?.ToString()! : string.Empty;
                            var transaction = new tbl_Transaction()
                            {
                                TransactionDate = dateOut,
                                SoldBy = soldBy?.Id,
                                TransactionStatus = 10,
                                CustomerId = customer.Id,
                                TransactionComment = commentStr,
                                ImportedId = transactionIdStr,
                            };

                            _context.tbl_Transactions.Add(transaction);
                            await _context.SaveChangesAsync();

                            bool groupHasError = false;

                            foreach (var detailInList in group)
                            {
                                totalTransactionDetails++;
                                try
                                {
                                    tbl_TransactionDetail? transactionEntity = null;

                                    var taxKey = GetKey("TaxName", p.ColumnMappingsList);
                                    var taxValueKey = GetKey("TaxPercent", p.ColumnMappingsList);
                                    string taxStr = !string.IsNullOrEmpty(taxKey) ? GetValue(detailInList, taxKey)?.ToString()! : string.Empty;
                                    string taxValueStr = !string.IsNullOrEmpty(taxValueKey) ? GetValue(detailInList, taxValueKey)?.ToString()! : string.Empty;
                                    var tax = _context.tbl_Taxes.FirstOrDefault(x => x.TaxValue == decimal.Parse(taxValueStr));

                                    if (tax == null)
                                    {
                                        tax = new tbl_Tax()
                                        {
                                            TaxDescription = taxStr,
                                            TaxValue = decimal.Parse(taxValueStr),
                                        };
                                        await _context.tbl_Taxes.AddAsync(tax);
                                        await _context.SaveChangesAsync();
                                    }


                                    var prodNameKey = GetKey("ProductName", p.ColumnMappingsList);
                                    string prodNameStr = !string.IsNullOrEmpty(prodNameKey) ? GetValue(detailInList, prodNameKey)?.ToString()! : string.Empty;
                                    var product = _context.tbl_Products.FirstOrDefault(x => x.ProductName == prodNameStr);
                                    if (product == null)
                                    {
                                        string description = BuildTransactionDescription(detailInList, p);
                                        messages.Add($"&#x1F4CC; {description} could not be processed as the product data is missing.");
                                        failedCount++;
                                        groupHasError = true;
                                        break;
                                    }

                                    var detailIdKey = GetKey("DetailId", p.ColumnMappingsList);
                                    string detailIdStr = !string.IsNullOrEmpty(detailIdKey) ? GetValue(detailInList, detailIdKey)?.ToString()! : string.Empty;

                                    if (string.IsNullOrEmpty(detailIdStr))
                                    {
                                        string description = BuildTransactionDescription(detailInList, p);
                                        messages.Add($"&#x1F4CC; {description} could not be processed due to missing Transaction detail Id.");
                                        failedCount++;
                                        groupHasError = true;
                                        break;
                                    }

                                    if (!string.IsNullOrEmpty(detailIdStr))
                                    {
                                        transactionEntity = await _context.tbl_TransactionDetails.FirstOrDefaultAsync(c => c.ImportedId == detailIdStr);
                                        if (transactionEntity != null)
                                        {
                                            string description = BuildTransactionDescription(detailInList, p);
                                            messages.Add($"&#x1F4CC; {description} could not be processed as the data is already imported.");
                                            failedCount++;
                                            groupHasError = true;
                                            break;
                                        }
                                        // Create new transaction  
                                        transactionEntity = new tbl_TransactionDetail();
                                        _context.tbl_TransactionDetails.Add(transactionEntity);
                                    }
                                    //else
                                    //{
                                    //    //// Check if name is unique (for creation)  
                                    //    //bool exists = await _context.tbl_TransactionDetails.AnyAsync(c => c.TransactionId.ToString() == transactionIdStr);
                                    //    //if (exists)
                                    //    //{
                                    //    //    string description = BuildTransactionDescription(detailInList, p);
                                    //    //    messages.Add($"&#x1F4CC; {description} could not be added as the transaction date '{transactionIdStr}' already exists.");
                                    //    //    failedCount++;
                                    //    //    groupHasError = true;
                                    //    //    break;
                                    //    //}

                                    //    // Create new transaction  
                                    //    transactionEntity = new tbl_TransactionDetail();
                                    //    _context.tbl_TransactionDetails.Add(transactionEntity);
                                    //}

                                    // Map fields from Excel data to transaction entity  
                                    foreach (var mapping in p.ColumnMappingsList)
                                    {
                                        string systemColumn = mapping.SystemColumn.ToLower();
                                        string fileColumn = mapping.SelectedFileColumn;
                                        if (string.IsNullOrEmpty(fileColumn))
                                            continue;

                                        object value = GetValue(detailInList, fileColumn);

                                        switch (systemColumn)
                                        {
                                            case "detailid":
                                                if (value != null)
                                                    transactionEntity.ImportedId = value.ToString();
                                                break;
                                            case "productname":
                                                if (value != null)
                                                    transactionEntity.ProductId = product.Id;
                                                break;
                                            case "qty":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal qty))
                                                    transactionEntity.Qty = qty;
                                                break;
                                            case "costexc":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal costExc))
                                                    transactionEntity.CostExc = costExc;
                                                break;
                                            case "costinc":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal costInc))
                                                    transactionEntity.CostInc = costInc;
                                                break;
                                            case "priceinc":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal priceInc))
                                                    transactionEntity.PriceInc = priceInc;
                                                break;
                                            case "priceexc":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal priceExc))
                                                    transactionEntity.PriceExc = priceExc;
                                                break;
                                            case "taxid":
                                                if (value != null)
                                                    transactionEntity.TaxId = tax.Id;
                                                break;
                                            case "taxpercent":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal taxPercent))
                                                {
                                                    transactionEntity.TaxPercent = taxPercent;
                                                    transactionEntity.TaxId = tax.Id;
                                                }
                                                break;
                                            case "discountid":
                                                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                                                    transactionEntity.DiscountId = value.ToString();
                                                break;
                                            case "discountpercent":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal discountPercent))
                                                    transactionEntity.DiscountPercent = discountPercent;
                                                break;
                                            case "transactionid":
                                                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                                                    transactionEntity.TransactionId = transaction.Id;
                                                break;
                                            case "totalpriceinc":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal totalPriceInc))
                                                    transactionEntity.TotalPriceInc = totalPriceInc;
                                                break;
                                            case "totalpriceexc":
                                                if (value != null && decimal.TryParse(value.ToString(), out decimal totalPriceExc))
                                                    transactionEntity.TotalPriceExc = totalPriceExc;
                                                break;
                                            case "sortorder":
                                                if (value != null && int.TryParse(value.ToString(), out int sortOrder))
                                                    transactionEntity.SortOrder = sortOrder;
                                                break;
                                            case "costincstate":
                                                if (value != null && bool.TryParse(value.ToString(), out bool costIncState))
                                                    transactionEntity.CostIncState = costIncState;
                                                break;
                                            case "specialpricingused":
                                                if (value != null && bool.TryParse(value.ToString(), out bool specialPricingUsed))
                                                    transactionEntity.SpecialPricingUsed = specialPricingUsed;
                                                break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string description = BuildTransactionDescription(detailInList, p);
                                    messages.Add($"&#x1F4CC; {description} could not be imported due to {ex.Message}");
                                    failedCount++;
                                    groupHasError = true;
                                    break;
                                }
                            }


                            if (!groupHasError)
                            {
                                await _context.SaveChangesAsync();
                                createdCount += group.Count();

                                //get transactiondetail just adde
                                var transactionDetails = await _context.tbl_TransactionDetails
                                    .Where(x => x.TransactionId == transaction.Id)
                                    .ToListAsync();

                                var totalSales = transactionDetails.Sum(x => x.TotalPriceInc);
                                //get paymentmode from imported excel and create it if it doesnt exist
                                var paymentModeKey = GetKey("PaymentType", p.ColumnMappingsList);
                                string paymentModeStr = !string.IsNullOrEmpty(paymentModeKey) ? GetValue(group.FirstOrDefault(), paymentModeKey)?.ToString()! : string.Empty;
                                var paymentMode = _context.tbl_PaymentModes.FirstOrDefault(x => x.Description == paymentModeStr);
                                if (paymentMode == null)
                                {
                                    paymentMode = new tbl_PaymentMode()
                                    {
                                        Description = paymentModeStr,
                                    };
                                    await _context.tbl_PaymentModes.AddAsync(paymentMode);
                                    await _context.SaveChangesAsync();
                                }


                                //create payment
                                var payment = new tbl_Payment()
                                {
                                    SaleId = transaction.Id,
                                    PaymentModeId = paymentMode.Id,
                                    Amount = totalSales
                                };

                                await _context.tbl_Payments.AddAsync(payment);
                                await _context.SaveChangesAsync();

                                //update transaction with transaction total
                                transaction.SaleTotal = totalSales;
                                _context.Entry(transaction).State = EntityState.Modified;

                                //_context.tbl_Transactions.Update(transaction);
                                await _context.SaveChangesAsync();

                            }
                            else
                            {
                                messages.Add($"Transaction group with TransactionId {transactionIdStr} was skipped due to errors.");
                            }
                        }

                        string summary = $"Total Transaction details Processed: {totalTransactionDetails}\n\nCreated: {createdCount}\nFailed: {failedCount}";
                        string resultMessage = string.Join("\n", messages);

                        var output = new ImportResultSummary
                        {
                            Summary = summary,
                            Errors = resultMessage
                        };
                        await scope.CommitAsync();
                        return ServiceResult<ImportResultSummary>.Success(output);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error while importing transaction details: {error}", ex);
                        await scope.RollbackAsync();
                        return ServiceResult<ImportResultSummary>.Failure(new ServerErrorException("Could not Import transaction details."));
                    }
                }
            });

        }
        private string BuildTransactionDescription(Dictionary<string, object> catData, ImportDataDto p)
        {
            List<string> parts = new List<string>();

            var detailIdKey = GetKey("DetailId", p.ColumnMappingsList);
            var transactionIdKey = GetKey("TransactionId", p.ColumnMappingsList);
            var qtyKey = GetKey("Qty", p.ColumnMappingsList);
            var costExcKey = GetKey("CostExc", p.ColumnMappingsList);
            var costIncKey = GetKey("CostInc", p.ColumnMappingsList);
            var priceIncKey = GetKey("PriceInc", p.ColumnMappingsList);
            var priceExcKey = GetKey("PriceExc", p.ColumnMappingsList);
            var taxIdKey = GetKey("TaxId", p.ColumnMappingsList);
            var taxPercentKey = GetKey("TaxPercent", p.ColumnMappingsList);
            var discountIdKey = GetKey("DiscountId", p.ColumnMappingsList);
            var discountPercentKey = GetKey("DiscountPercent", p.ColumnMappingsList);
            var totalPriceIncKey = GetKey("TotalPriceInc", p.ColumnMappingsList);
            var totalPriceExcKey = GetKey("TotalPriceExc", p.ColumnMappingsList);
            var sortOrderKey = GetKey("SortOrder", p.ColumnMappingsList);
            var specialPricingUsedKey = GetKey("SpecialPricingUsed", p.ColumnMappingsList);

            var detailIdVal = !string.IsNullOrEmpty(detailIdKey) ? GetValue(catData, detailIdKey)?.ToString() : "";
            var transactionIdVal = !string.IsNullOrEmpty(transactionIdKey) ? GetValue(catData, transactionIdKey)?.ToString() : "";
            var qtyVal = !string.IsNullOrEmpty(qtyKey) ? GetValue(catData, qtyKey)?.ToString() : "";
            var costExcVal = !string.IsNullOrEmpty(costExcKey) ? GetValue(catData, costExcKey)?.ToString() : "";
            var costIncVal = !string.IsNullOrEmpty(costIncKey) ? GetValue(catData, costIncKey)?.ToString() : "";
            var priceIncVal = !string.IsNullOrEmpty(priceIncKey) ? GetValue(catData, priceIncKey)?.ToString() : "";
            var priceExcVal = !string.IsNullOrEmpty(priceExcKey) ? GetValue(catData, priceExcKey)?.ToString() : "";
            var taxIdVal = !string.IsNullOrEmpty(taxIdKey) ? GetValue(catData, taxIdKey)?.ToString() : "";
            var taxPercentVal = !string.IsNullOrEmpty(taxPercentKey) ? GetValue(catData, taxPercentKey)?.ToString() : "";
            var discountIdVal = !string.IsNullOrEmpty(discountIdKey) ? GetValue(catData, discountIdKey)?.ToString() : "";
            var discountPercentVal = !string.IsNullOrEmpty(discountPercentKey) ? GetValue(catData, discountPercentKey)?.ToString() : "";
            var totalPriceIncVal = !string.IsNullOrEmpty(totalPriceIncKey) ? GetValue(catData, totalPriceIncKey)?.ToString() : "";
            var totalPriceExcVal = !string.IsNullOrEmpty(totalPriceExcKey) ? GetValue(catData, totalPriceExcKey)?.ToString() : "";
            var sortOrderVal = !string.IsNullOrEmpty(sortOrderKey) ? GetValue(catData, sortOrderKey)?.ToString() : "";
            var specialPricingUsedVal = !string.IsNullOrEmpty(specialPricingUsedKey) ? GetValue(catData, specialPricingUsedKey)?.ToString() : "";

            if (!string.IsNullOrEmpty(detailIdVal))
                parts.Add($"DetailId: {detailIdVal}");
            if (!string.IsNullOrEmpty(transactionIdVal))
                parts.Add($"TransactionId: {transactionIdVal}");
            if (!string.IsNullOrEmpty(qtyVal))
                parts.Add($"Qty: {qtyVal}");
            if (!string.IsNullOrEmpty(costExcVal))
                parts.Add($"CostExc: {costExcVal}");
            if (!string.IsNullOrEmpty(costIncVal))
                parts.Add($"CostInc: {costIncVal}");
            if (!string.IsNullOrEmpty(priceIncVal))
                parts.Add($"PriceInc: {priceIncVal}");
            if (!string.IsNullOrEmpty(priceExcVal))
                parts.Add($"PriceExc: {priceExcVal}");
            if (!string.IsNullOrEmpty(taxIdVal))
                parts.Add($"TaxId: {taxIdVal}");
            if (!string.IsNullOrEmpty(taxPercentVal))
                parts.Add($"TaxPercent: {taxPercentVal}");
            if (!string.IsNullOrEmpty(discountIdVal))
                parts.Add($"DiscountId: {discountIdVal}");
            if (!string.IsNullOrEmpty(discountPercentVal))
                parts.Add($"DiscountPercent: {discountPercentVal}");
            if (!string.IsNullOrEmpty(totalPriceIncVal))
                parts.Add($"TotalPriceInc: {totalPriceIncVal}");
            if (!string.IsNullOrEmpty(totalPriceExcVal))
                parts.Add($"TotalPriceExc: {totalPriceExcVal}");
            if (!string.IsNullOrEmpty(sortOrderVal))
                parts.Add($"SortOrder: {sortOrderVal}");
            if (!string.IsNullOrEmpty(specialPricingUsedVal))
                parts.Add($"SpecialPricingUsed: {specialPricingUsedVal}");

            return "TransactionDetail [" + string.Join(", ", parts) + "]";
        }
        private object GetValue(Dictionary<string, object> item, string key)
        {
            return item.TryGetValue(key, out object? value) ? (value == null ? "" : value) : "";
        }
        private string GetKey(string columnName, List<ColumnMapping> mappings)
        {
            return mappings.FirstOrDefault(x => x.SystemColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))?.SelectedFileColumn ?? "";
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
