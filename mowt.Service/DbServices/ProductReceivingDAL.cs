using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Service.Extensions;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.ProductStructureDtos;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DbServices
{
	public class ProductReceivingDAL : IProductReceivingDAL
	{
		private readonly mowtDbContext _context;
		private readonly ILogger<CustomerDAL> _logger;
		private readonly IProductsDAL _productsDAL;

		public ProductReceivingDAL(ILogger<CustomerDAL> logger, mowtDbContext context, IProductsDAL productsDAL)
		{
			_logger = logger;
			_context = context;
			_productsDAL = productsDAL;
		}

		#region Method for reading Product Receiving from DB using GRNSupplierNumber
		public async Task<ServiceResult<List<ProductReceivingDto>>> GetProductReceivingDetailFromDBPerGRNnumber(string GRNSupplierNumber)
		{
			if (string.IsNullOrWhiteSpace(GRNSupplierNumber)) return ServiceResult<List<ProductReceivingDto>>.Failure(
																	new BadRequestException("GRN Supplier number is required"));
			try
			{
				var receivingDetails = await _context.tbl_ProductReceivings.AsNoTracking()
										.Where(s => s.GrnsupplierNumber == GRNSupplierNumber)
										.OrderByDescending(c => c.DateReceived).ToListAsync();

				var receivingDetailsDto = receivingDetails.Adapt<List<ProductReceivingDto>>();
				var productIds = receivingDetailsDto.Select(r => r.ProductId).Distinct().ToList();
				var products = await _context.tbl_Products.AsNoTracking()
									.Where(p => productIds.Contains(p.Id))
									.ToListAsync();
				var productsDto = products.Adapt<List<ProductsDto>>();
				foreach (var receiving in receivingDetailsDto)
				{
					receiving.Product = productsDto.FirstOrDefault(p => p.Id == receiving.ProductId);
				}

				return ServiceResult<List<ProductReceivingDto>>.Success(receivingDetailsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching product receiving details for GRNSupplierNumber: {GRNSupplierNumber}", GRNSupplierNumber);
				return ServiceResult<List<ProductReceivingDto>>.Failure(
					new ServerErrorException("Could not fetch product receiving details."));
			}
		}
		#endregion

		#region Read Products Received from Database based on date range
		public async Task<ServiceResult<List<ProductReceivingDto>>> GetProductsReceivedFromDBUsingDateRange(DateTime startDate, DateTime endDate)
		{
			if (startDate > endDate) return ServiceResult<List<ProductReceivingDto>>.Failure(
											new BadRequestException("Start date cannot be higher than end date"));
			try
			{
				var receivingDetails = await _context.tbl_ProductReceivings.AsNoTracking()
										.Where(s => s.DateReceived >= startDate && endDate >= s.DateReceived)
										.OrderByDescending(c => c.Id).ToListAsync();

				var receivingDetailsDto = receivingDetails.Adapt<List<ProductReceivingDto>>();

				return ServiceResult<List<ProductReceivingDto>>.Success(receivingDetailsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching product receiving details for date range: {StartDate} - {EndDate}", startDate, endDate);
				return ServiceResult<List<ProductReceivingDto>>.Failure(
					new ServerErrorException("Could not fetch product receiving details."));
			}
		}
		#endregion

		#region Method for adding ProductReceiving to the DB
		public async Task<ServiceResult<ProductReceivingDto>> AddOProductReceivingDetailToDB(ProductReceivingDto p)
		{
			if (p == null) return ServiceResult<ProductReceivingDto>.Failure(
				new BadRequestException("Product receiving data is required."));

			try
			{
				var productReceiving = p.Adapt<tbl_ProductReceiving>();

				await _context.AddAsync(productReceiving);

				await _context.SaveChangesAsync();

				var createdDto = productReceiving.Adapt<ProductReceivingDto>();

				return ServiceResult<ProductReceivingDto>.Success(createdDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while creating product receiving data for ProductId: {ProductId}", p?.ProductId);
				return ServiceResult<ProductReceivingDto>.Failure(
					new ServerErrorException("Could not create product receiving data."));
			}
		}
		#endregion

		#region Multiple ProductReceiving to the DB
		public async Task<ServiceResult<List<ProductReceivingDto>>> ReceiveMultipleProducts(List<ProductReceivingDto> p, List<StockParam> s, List<CostPriceChange>? c)
		{
			if (p == null || p.Count == 0) return ServiceResult<List<ProductReceivingDto>>.Failure(
				new BadRequestException("Product receiving data is required."));

			try
			{
				var strategy = _context.Database.CreateExecutionStrategy();

				return await strategy.ExecuteAsync(async () => await ReceiveMultipleProductsAsync(p, s, c));

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while creating multiple product receiving records.");
				return ServiceResult<List<ProductReceivingDto>>.Failure(
					new ServerErrorException("Could not create product receiving data."));
			}
		}
		#endregion

		#region ReceiveMultipleProductsAsync
		private async Task<ServiceResult<List<ProductReceivingDto>>> ReceiveMultipleProductsAsync(List<ProductReceivingDto> p, List<StockParam> stockParamList, List<CostPriceChange>? costChanges)
		{
			var productReceivingList = p.Adapt<List<tbl_ProductReceiving>>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				await _productsDAL.UpdateStockFromProductReceiving(stockParamList);

				if (costChanges != null) await _productsDAL.UpdateProductCostPrices(costChanges);

				await _context.AddRangeAsync(productReceivingList);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				var createdDto = productReceivingList.Adapt<List<ProductReceivingDto>>();
				return ServiceResult<List<ProductReceivingDto>>.Success(createdDto);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error while creating multiple product receiving records in transaction.");
				return ServiceResult<List<ProductReceivingDto>>.Failure(
					new ServerErrorException("Could not create product receiving data."));
			}
		}
		#endregion

		#region Read Products Received from Database based on date range and GRNSupplierNumber
		public async Task<ServiceResult<List<ProductReceivingDto>>> GetProductsReceivedFromDBUsingDateRangeAndGRNSupplierNumber(DateTime @startDate, DateTime @endDate, string GRNSupplierNumber)
		{
			if (string.IsNullOrWhiteSpace(GRNSupplierNumber)) return ServiceResult<List<ProductReceivingDto>>.Failure(
																	new BadRequestException("Supplier number is required."));

			if (startDate > endDate) return ServiceResult<List<ProductReceivingDto>>.Failure(
											new BadRequestException("Start date cannot be higher than end date."));

			try
			{
				var receivingDetails = await _context.tbl_ProductReceivings.AsNoTracking()
										.Where(s => s.GrnsupplierNumber == GRNSupplierNumber
										&& s.DateReceived >= startDate && endDate >= s.DateReceived)
										.OrderByDescending(c => c.Id).ToListAsync();

				var receivingDetailsDto = receivingDetails.Adapt<List<ProductReceivingDto>>();

				return ServiceResult<List<ProductReceivingDto>>.Success(receivingDetailsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching product receiving details for GRNSupplierNumber: {GRNSupplierNumber} and date range: {StartDate} - {EndDate}", GRNSupplierNumber, startDate, endDate);
				return ServiceResult<List<ProductReceivingDto>>.Failure(
					new ServerErrorException("Could not fetch product receiving details."));
			}
		}
		#endregion

		#region Search Product Receiving Detail from DB
		public async Task<ServiceResult<PaginationDetails<ProductReceivingDto>>> SearchProductReceivingDetailFromDB(string? receiveStockId, string? supplierAccount, string? keywords = "", string? barCode = "", int offset = 1, int limit = 1, CancellationToken token = default)
		{
			var query = _context.tbl_ProductReceivings.AsNoTracking();
			try
			{
				if (!string.IsNullOrEmpty(receiveStockId))
				{
					query = query.Where(x => x.Id == receiveStockId);
				}
				if (!string.IsNullOrEmpty(supplierAccount))
				{
					query = query.Where(x => x.SupplierAccount == supplierAccount);
				}

				if (!string.IsNullOrWhiteSpace(keywords))
				{
					query = query.Where(x => x.GrnsupplierNumber != null && x.GrnsupplierNumber.Contains(keywords));
				}

				var receivingDetails = await query.ToPaginatedResultAsync(offset, limit, token, null, true);

				var receivingDetailsDto = receivingDetails.Adapt<PaginationDetails<ProductReceivingDto>>();

				return ServiceResult<PaginationDetails<ProductReceivingDto>>.Success(receivingDetailsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while searching product receiving details. ReceiveStockId: {ReceiveStockId}, SupplierAccount: {SupplierAccount}, Keywords: {Keywords}", receiveStockId, supplierAccount, keywords);
				return ServiceResult<PaginationDetails<ProductReceivingDto>>.Failure(
					new ServerErrorException("Could not search product receiving details."));
			}
		}
		#endregion
	}
}
