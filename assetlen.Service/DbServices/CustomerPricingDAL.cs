using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
	public class CustomerPricingDAL : ICustomerPricingDAL
	{
		private readonly mowtDbContext _context;
		private readonly ILogger<CustomerPricingDAL> _logger;

		public CustomerPricingDAL(ILogger<CustomerPricingDAL> logger, mowtDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		#region Get All Customer Pricing with Pagination
		public async Task<ServiceResult<PaginationDetails<PricingsDto>>> GetAllCustomerBasedPricingFromDB(int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
		{
			try
			{
				var customerPricings = await _context.tbl_CustomerPricings.AsNoTracking()
					.ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

				var pricingsDto = customerPricings.Adapt<PaginationDetails<PricingsDto>>();

				foreach (var p in pricingsDto.Data)
				{
					p.CustomerName = await _context.tbl_Customers
						.Where(x => x.Id == p.CustomerId)
						.Select(x => x.FullName)
						.FirstOrDefaultAsync();

					p.ProductName = await _context.tbl_Products
						.Where(x => x.Id == p.ProductId)
						.Select(x => x.ProductName)
						.FirstOrDefaultAsync();
				}
				return ServiceResult<PaginationDetails<PricingsDto>>.Success(pricingsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching customer pricings from database.");
				return ServiceResult<PaginationDetails<PricingsDto>>.Failure(
					new ServerErrorException("Could not fetch customer pricings."));
			}
		}
		#endregion

		#region Read CustomersPricing from Database based on CustomerID and ProductID
		public async Task<ServiceResult<List<PricingsDto>>> GetPricingListByCustomerIdAndProductId(string customerId, string productId)
		{
			try
			{
				var customerPricing = await _context.tbl_CustomerPricings
								.Where(c => c.ProductId == productId && c.CustomerId == customerId)
								.ToListAsync();

				var pricingsDto = customerPricing.Adapt<List<PricingsDto>>();

				foreach (var p in pricingsDto)
				{
					p.CustomerName = await _context.tbl_Customers
						.Where(x => x.Id == p.CustomerId)
						.Select(x => x.FullName)
						.FirstOrDefaultAsync();

					p.ProductName = await _context.tbl_Products
						.Where(x => x.Id == p.ProductId)
						.Select(x => x.ProductName)
						.FirstOrDefaultAsync();
				}

				return ServiceResult<List<PricingsDto>>.Success(pricingsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching pricing for customer {CustomerId} and product {ProductId}", customerId, productId);
				return ServiceResult<List<PricingsDto>>.Failure(
					new ServerErrorException("Could not fetch customer pricing."));
			}

		}
		#endregion

		#region Read customerPricing from Database based on customerPricingID
		public async Task<ServiceResult<PricingsDto>> GetCustomerPricingByID(string id)
		{
			try
			{
				var customerPricing = await _context.tbl_CustomerPricings.FirstOrDefaultAsync(x => x.Id == id);

				if (customerPricing == null)
				{
					_logger.LogError("Customer pricing with ID: {CustomerPricingId} not found.", id);
					return ServiceResult<PricingsDto>.Failure(
						new NotFoundException($"Customer pricing with ID: {id} not found."));
				}
				var pricingDto = customerPricing.Adapt<PricingsDto>();

				pricingDto.ProductName = await _context.tbl_Products
					.Where(x => x.Id == pricingDto.ProductId)
					.Select(x => x.ProductName)
					.FirstOrDefaultAsync();

				pricingDto.CustomerName = await _context.tbl_Customers
					.Where(x => x.Id == pricingDto.CustomerId)
					.Select(x => x.FullName)
					.FirstOrDefaultAsync();

				return ServiceResult<PricingsDto>.Success(pricingDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching customer pricing with ID {CustomerPricingId}", id);
				return ServiceResult<PricingsDto>.Failure(
					new ServerErrorException("Could not fetch customer pricing."));
			}

		}
		#endregion

		#region Create New CustomerPrincing

		public async Task<ServiceResult<PricingsDto>> AddCustomerPricing(PricingsDto cupDto)
		{

			if (cupDto == null) return ServiceResult<PricingsDto>.Failure(
				new BadRequestException("Customer pricing data is required."));

			try
			{
				var cup = cupDto.Adapt<tbl_CustomerPricing>();

				await _context.AddAsync(cup);

				await _context.SaveChangesAsync();

				var createdCupDto = cup.Adapt<PricingsDto>();

				return ServiceResult<PricingsDto>.Success(createdCupDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while creating customer pricing.");
				return ServiceResult<PricingsDto>.Failure(
					new ServerErrorException("Could not create customer pricing."));
			}
		}
		#endregion

		#region update CustomerPricing in the  DB
		public async Task<ServiceResult<PricingsDto>> UpdateCustomerPricing(PricingsDto cu)
		{
			if (cu == null) return ServiceResult<PricingsDto>.Failure(
								new BadRequestException("Customer Pricing data is required."));


			if (string.IsNullOrEmpty(cu.Id)) return ServiceResult<PricingsDto>.Failure(
				new BadRequestException($"Customer pricing Id is required."));

			try
			{
				var pricingInDb = await _context.tbl_CustomerPricings
											.FirstOrDefaultAsync(c => c.Id == cu.Id);

				if (pricingInDb == null) return ServiceResult<PricingsDto>.Failure(
											new NotFoundException($"Customer Pricing with ID {cu.Id} not found."));

				// Update properties dynamically
				pricingInDb.CustomerId = cu.CustomerId != default ? cu.CustomerId : pricingInDb.CustomerId;
				pricingInDb.ProductId = cu.ProductId != default ? cu.ProductId : pricingInDb.ProductId;
				pricingInDb.PriceGroupId = cu.PriceGroupId != default ? cu.PriceGroupId : pricingInDb.PriceGroupId;
				pricingInDb.PriceInc = cu.PriceInc != default ? cu.PriceInc : pricingInDb.PriceInc;
				pricingInDb.PriceExc = cu.PriceExc != default ? cu.PriceExc : pricingInDb.PriceExc;
				pricingInDb.TaxId = cu.TaxId != default ? cu.TaxId : pricingInDb.TaxId;
				pricingInDb.SortOrder = cu.SortOrder != default ? cu.SortOrder : pricingInDb.SortOrder;
				pricingInDb.CostInc = cu.CostInc != default ? cu.CostInc : pricingInDb.CostInc;
				pricingInDb.CostExc = cu.CostExc != default ? cu.CostExc : pricingInDb.CostExc;

				_context.tbl_CustomerPricings.Update(pricingInDb);
				await _context.SaveChangesAsync();

				var updatedPricingDto = pricingInDb.Adapt<PricingsDto>();

				return ServiceResult<PricingsDto>.Success(updatedPricingDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while updating customer pricing with ID {CustomerPricingId}", cu.Id);
				return ServiceResult<PricingsDto>.Failure(
					new ServerErrorException("Could not update customer pricing."));
			}
		}
		#endregion

		#region delete CustomerPricing in the  DB based on productID
		public async Task<ServiceResult<bool>> DeleteCustomerPricingListByProductId(string productId)
		{
			var pricingInDb = await _context.tbl_CustomerPricings
									.Where(c => c.ProductId == productId)
									.ToListAsync();

			if (pricingInDb == null) return ServiceResult<bool>
					.Failure(new NotFoundException($"Customer pricing for product with ID: {productId} not found."));

			try
			{
				//soft delete
				foreach (var pricing in pricingInDb)
				{
					pricing.IsDeleted = true;
					_context.tbl_CustomerPricings.Update(pricing);
				}

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while deleting customer pricing for product with ID {ProductId}", productId);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete customer pricing for product."));
			}
		}
		#endregion

		#region delete CustomerPricing in the  DB based on customerID
		public async Task<ServiceResult<bool>> DeleteCustomerPricingListByCustomerId(string customerId)
		{
			var pricingInDb = await _context.tbl_CustomerPricings
									.Where(c => c.CustomerId == customerId)
									.ToListAsync();
			if (pricingInDb == null) return ServiceResult<bool>
					.Failure(new NotFoundException($"Customer pricing for customer with ID: {customerId} not found."));
			try
			{
				//soft delete
				foreach (var pricing in pricingInDb)
				{
					pricing.IsDeleted = true;
					_context.tbl_CustomerPricings.Update(pricing);
				}
				await _context.SaveChangesAsync();
				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while deleting customer pricing for customer with ID {CustomerId}", customerId);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete customer pricing for customer."));
			}
		}
		#endregion

		#region Delete CustomerPricing in the  DB based on customerPricingID
		public async Task<ServiceResult<bool>> DeleteCustomerPricingById(string id)
		{
			var priceInDb = await _context.tbl_CustomerPricings
									.FirstOrDefaultAsync(c => c.Id == id);
			if (priceInDb == null)
				return ServiceResult<bool>.Failure(
				new NotFoundException($"Customer pricing with ID: {id} not found."));

			try
			{
				//soft delete
				priceInDb.IsDeleted = true;
				_context.tbl_CustomerPricings.Update(priceInDb);
				await _context.SaveChangesAsync();
				return ServiceResult<bool>.Success(true);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while deleting customer pricing with ID {CustomerPricingId}", id);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete customer pricing."));
			}

		}
		#endregion

		#region Read customerPricing from Database based on customerID
		public async Task<ServiceResult<List<PricingsDto>>> GetCustomerPricingListByCustomerID(string customerId)
		{
			try
			{
				var pricings = await _context.tbl_CustomerPricings.AsNoTracking()
								.Where(c => c.CustomerId == customerId)
								.OrderBy(c => c.SortOrder).ToListAsync();
				var prcingDtos = pricings.Adapt<List<PricingsDto>>();

				foreach (var p in prcingDtos)
				{
					p.CustomerName = await _context.tbl_Customers
						.Where(x => x.Id == p.CustomerId)
						.Select(x => x.FullName)
						.FirstOrDefaultAsync();

					p.ProductName = await _context.tbl_Products
						.Where(x => x.Id == p.ProductId)
						.Select(x => x.ProductName)
						.FirstOrDefaultAsync();
				}
				return ServiceResult<List<PricingsDto>>.Success(prcingDtos);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching customer pricings for customer ID {CustomerId}", customerId);
				return ServiceResult<List<PricingsDto>>.Failure(
					new ServerErrorException("Could not fetch customer pricings."));
			}

		}
		#endregion

		#region Read customerPricing from Database based on ProductID
		public async Task<ServiceResult<List<PricingsDto>>> GetCustomerPricingListByProductID(string productId)
		{
			try
			{
				var pricings = await _context.tbl_CustomerPricings.AsNoTracking()
										.Where(c => c.ProductId == productId)
										.OrderBy(c => c.SortOrder).ToListAsync();

				var pricingsDto = pricings.Adapt<List<PricingsDto>>();
				foreach (var p in pricingsDto)
				{
					p.CustomerName = await _context.tbl_Customers
						.Where(x => x.Id == p.CustomerId)
						.Select(x => x.FullName)
						.FirstOrDefaultAsync();
					p.ProductName = await _context.tbl_Products
						.Where(x => x.Id == p.ProductId)
						.Select(x => x.ProductName)
						.FirstOrDefaultAsync();
				}

				return ServiceResult<List<PricingsDto>>.Success(pricingsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while fetching customer pricings for product ID {ProductId}", productId);
				return ServiceResult<List<PricingsDto>>.Failure(
					new ServerErrorException("Could not fetch customer pricings."));
			}

		}
		#endregion

		#region Search Customer Pricing with Pagination
		public async Task<ServiceResult<PaginationDetails<PricingsDto>>> SearchCustomerBasedPricingInDb(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
		{
			try
			{
				var query = _context.tbl_CustomerPricings.AsNoTracking();

				if (!string.IsNullOrEmpty(keywords))
				{
					// Join with Customer and Product tables to search by names
					query = from pricing in _context.tbl_CustomerPricings
							join customer in _context.tbl_Customers on pricing.CustomerId equals customer.Id into customerJoin
							from customer in customerJoin.DefaultIfEmpty()
							join product in _context.tbl_Products on pricing.ProductId equals product.Id into productJoin
							from product in productJoin.DefaultIfEmpty()
							where pricing.Id.ToString().Contains(keywords)
								|| (customer != null && customer.FullName != null && customer.FullName.Contains(keywords))
								|| (product != null && product.ProductName != null && product.ProductName.Contains(keywords))
							select pricing;
				}

				var customerPricings = await query.ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

				var pricingsDto = customerPricings.Adapt<PaginationDetails<PricingsDto>>();

				foreach (var p in pricingsDto.Data)
				{
					p.CustomerName = await _context.tbl_Customers
						.Where(x => x.Id == p.CustomerId)
						.Select(x => x.FullName)
						.FirstOrDefaultAsync();

					p.ProductName = await _context.tbl_Products
						.Where(x => x.Id == p.ProductId)
						.Select(x => x.ProductName)
						.FirstOrDefaultAsync();
				}

				return ServiceResult<PaginationDetails<PricingsDto>>.Success(pricingsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while searching customer pricings with keywords '{Keywords}'", keywords);
				return ServiceResult<PaginationDetails<PricingsDto>>.Failure(
					new ServerErrorException("Could not search customer pricings."));
			}
		}
		#endregion

		#region Create new or update exisiting customer pricing
		public async Task<ServiceResult<PricingsDto>> CreateUpdateCustomerPricing(PricingsDto pricingDto)
		{
			if (pricingDto == null) return ServiceResult<PricingsDto>.Failure(
				new BadRequestException("Customer pricing data is required."));
			try
			{
				var pricingExists = await _context.tbl_CustomerPricings
					.AnyAsync(c => c.Id == pricingDto.Id);
				var pricingOutDto = new PricingsDto();
				if (pricingExists)
				{
					var result = await UpdateCustomerPricing(pricingDto);
					if (!result.IsSuccess)
					{
						return ServiceResult<PricingsDto>.Failure(result.Error);
					}
					else
					{
						pricingOutDto = result.Data;
					}
				}
				else
				{
					var result = await AddCustomerPricing(pricingDto);
					if (!result.IsSuccess)
					{
						return ServiceResult<PricingsDto>.Failure(result.Error);
					}
					else
					{
						pricingOutDto = result.Data;
					}
				}

				pricingOutDto.CustomerName = await _context.tbl_Customers
						.Where(x => x.Id == pricingOutDto.CustomerId)
						.Select(x => x.FullName)
						.FirstOrDefaultAsync();
				pricingOutDto.ProductName = await _context.tbl_Products
						.Where(x => x.Id == pricingOutDto.ProductId)
						.Select(x => x.ProductName)
						.FirstOrDefaultAsync();
				return ServiceResult<PricingsDto>.Success(pricingOutDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while creating or updating customer pricing.");
				return ServiceResult<PricingsDto>.Failure(
					new ServerErrorException("Could not create or update customer pricing."));
			}
		}
		#endregion

	}
}
