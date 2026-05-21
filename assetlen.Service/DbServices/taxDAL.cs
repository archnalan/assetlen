using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using MailKit.Search;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
	public class taxDAL : ItaxDAL
	{
		private readonly AssetlenDbContext _context;
		private readonly ILogger<taxDAL> _logger;

		public taxDAL(AssetlenDbContext billDbContext, ILogger<taxDAL> logger)
		{
			_context = billDbContext;
			_logger = logger;
		}

		#region Read Tax from Database based on TaxID
		public async Task<ServiceResult<taxDto>> GetTaxFromDBbasedOnTaxID(string taxId)
		{
			try
			{
				var result = await _context.tbl_Taxes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == taxId);

				return ServiceResult<taxDto>.Success(result.Adapt<taxDto>());

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching tax with ID {TaxId}: {Error}", taxId, ex);
				return ServiceResult<taxDto>.Failure(
					new ServerErrorException("Could not fetch tax."));
			}
		}
		#endregion

		#region Read TaxID from Database based on TaxDescription
		public async Task<ServiceResult<string>> GetTaxIDFromDBbasedOnTaxDescription(string taxDescription)
		{
			try
			{
				var result = await _context.tbl_Taxes.AsNoTracking().FirstOrDefaultAsync(x => x.TaxDescription == taxDescription);
				if (result != null)
				{
					return ServiceResult<string>.Success(result.Id);
				}
				return ServiceResult<string>.Failure(new NotFoundException("Tax with the Requested Description not found"));

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching tax ID for description '{TaxDescription}': {Error}", taxDescription, ex);
				return ServiceResult<string>.Failure(
					new ServerErrorException("Could not fetch tax ID."));
			}
		}
		#endregion

		#region Read all Tax from Database
		public async Task<ServiceResult<List<taxDto>>> GetAllTaxFromDB()
		{
			try
			{
				var result = await _context.tbl_Taxes.AsNoTracking().ToListAsync();
				return ServiceResult<List<taxDto>>.Success(result.Adapt<List<taxDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching taxes: {Error}", ex);
				return ServiceResult<List<taxDto>>.Failure(
					new ServerErrorException("Could not fetch taxes."));
			}
		}
		#endregion

		#region Check if Tax Item already in use Tax  from Database
		public async Task<ServiceResult<bool>> GetTop1TaxFromSalesDBUsingTaxID(string taxId)
		{

			var result = await _context.tbl_TransactionDetails.AnyAsync(x => x.TaxId == taxId);
			return ServiceResult<bool>.Success(result);
		}
		#endregion

		#region Check if Tax Item already in use Tax  from Database
		public async Task<ServiceResult<bool>> GetTop1TaxFromProductsDBUsingTaxID(string taxId)
		{
			try
			{
				var result = await _context.tbl_Products.AnyAsync(x => x.TaxId == taxId);

				return ServiceResult<bool>.Success(result);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while checking if tax is used in products: {Error}", ex);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not check if tax is used in products."));
			}
		}
		#endregion

		#region Create New Taxin DB

		public async Task<ServiceResult<taxDto>> CreateNewTax(taxDto taxDto)
		{
			if (taxDto == null) return ServiceResult<taxDto>.Failure(
			   new BadRequestException("Tax data is required."));

			var tax = taxDto.Adapt<tbl_Tax>();
			try
			{
				var created = await _context.AddAsync(tax);
				await _context.SaveChangesAsync();
				return ServiceResult<taxDto>.Success(tax.Adapt<taxDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while creating tax: {Error}", ex);
				return ServiceResult<taxDto>.Failure(
					new ServerErrorException("Could not create tax."));
			}
		}
		#endregion
		#region update Tax in the  DB

		public async Task<ServiceResult<taxDto>> UpdateTaxinDBbasedOnTaxID(taxDto taxDto)
		{
			try
			{
				var tax = taxDto.Adapt<tbl_Tax>();

				var taxFromDb = await _context.tbl_Taxes.FirstOrDefaultAsync(x => x.Id == tax.Id);
				if (taxFromDb == null) return ServiceResult<taxDto>.Failure(new NotFoundException($"Tax with id {tax.Id}  was not found"));

				taxFromDb.TaxValue = tax.TaxValue ?? taxFromDb.TaxValue;
				taxFromDb.TaxDescription = tax.TaxDescription ?? taxFromDb.TaxDescription;

				await _context.SaveChangesAsync();
				return ServiceResult<taxDto>.Success(tax.Adapt<taxDto>());

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while updating tax: {Error}", ex);
				return ServiceResult<taxDto>.Failure(
					new ServerErrorException("Could not update tax."));
			}
		}
		#endregion
		#region Delete Tax in the  DB

		public async Task<ServiceResult<bool>> DeleteTaxinDBbasedOnTaxID(string taxId)
		{
			try
			{
				var taxFromDb = await _context.tbl_Taxes.FirstOrDefaultAsync(x => x.Id == taxId);
				if (taxFromDb == null) return ServiceResult<bool>.Failure(new NotFoundException($"Tax with id {taxId}  was not found"));

				taxFromDb.IsDeleted = true;
				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while deleting tax with ID {TaxId}: {Error}", taxId, ex);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete tax."));
			}
		}
		#endregion
		#region Delete Tax in the  DB

		public async Task<ServiceResult<bool>> HardDeleteTaxinDBbasedOnID(string taxId)
		{

			var taxFromDb = await _context.tbl_Taxes.FirstOrDefaultAsync(x => x.Id == taxId);
			if (taxFromDb == null) return ServiceResult<bool>.Failure(new NotFoundException($"Tax with id {taxId}  was not found"));

			_context.Remove(taxFromDb);
			await _context.SaveChangesAsync();

			return ServiceResult<bool>.Success(true);



		}
		#endregion

		#region Search Tax in the  DB
		public async Task<ServiceResult<List<taxDto>>> SearchTaxFromDB(string searchText)
		{
			try
			{
				var result = await _context.tbl_Taxes
					.AsNoTracking()
					.Where(x => x.TaxDescription != null && x.TaxDescription.Contains(searchText) ||
					x.TaxValue.HasValue && x.TaxValue.Value.ToString().Contains(searchText) ||
					x.Id.ToString().Contains(searchText))
					.ToListAsync();

				result.ForEach(x => x.TaxDescription = $"{x.TaxDescription} ({(x.TaxValue ?? 0m).ToString("N2")}%)");

				return ServiceResult<List<taxDto>>.Success(result.Adapt<List<taxDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching taxes with keyword '{SearchText}': {Error}", searchText, ex);
				return ServiceResult<List<taxDto>>.Failure(
					new ServerErrorException("Could not search taxes."));
			}
		}
		#endregion
		#region Search tax for combo boxes
		public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchTaxesForComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
		{
			try
			{
				IQueryable<tbl_Tax> query = _context.tbl_Taxes;

				if (!string.IsNullOrEmpty(keywords))
				{
					query = query.
							 Where(x => x.TaxDescription.ToString() == keywords ||
							 x.TaxValue != null && x.TaxValue.ToString() == (keywords)
							 );
				}

				var tax = await query.AsNoTracking()
										  .Select(x => new ComboBoxDto
										  {
											  Id = x.Id,
											  IdString = x.Id.ToString(),
											  ValueText = $"{x.TaxDescription} ({x.TaxValue}%)" ?? string.Empty
										  })
										  .OrderBy(c => c.ValueText)
										  .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

				var taxDto = tax.Adapt<PaginationDetails<ComboBoxDto>>();

				return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(taxDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching taxes for combobox: {Error}", ex);
				return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
					new ServerErrorException("Could not search taxes."));
			}
		}
		#endregion
	}
}
