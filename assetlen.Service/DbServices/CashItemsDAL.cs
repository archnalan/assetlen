using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels.Users;
using MailKit.Search;
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

namespace assetlen.Service.DbServices
{
	public class CashItemsDAL : ICashItemsDAL
	{
		private readonly AssetlenDbContext _context;
		private readonly ILogger<CashItemsDAL> _logger;

		public CashItemsDAL(AssetlenDbContext context, ILogger<CashItemsDAL> logger)
		{
			_context = context;

			_logger = logger;
		}

		#region Read CashItems from Database
		public async Task<ServiceResult<List<CashItemsDto>>> GetCashItemsFromDB()
		{
			try
			{
				var cashItems = await _context.tbl_CashItems.OrderByDescending(x => x.Amount).ToListAsync();

				var cashItemsDto = cashItems.Select(item => item.Adapt<CashItemsDto>()).ToList();

				return ServiceResult<List<CashItemsDto>>.Success(cashItemsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching cash items from database: {Error}", ex);
				return ServiceResult<List<CashItemsDto>>.Failure(
					new ServerErrorException("Could not fetch cash items."));
			}
		}
		#endregion

		#region Read CashItems from Database based on Id
		public async Task<ServiceResult<CashItemsDto>> GetCashItemBasedOnID(string cashItemId)
		{
			try
			{
				var cashItem = await _context.tbl_CashItems.FindAsync(cashItemId);

				if (cashItem == null)
				{
					_logger.LogError("Cash item with ID: {CashItemId} not found.", cashItemId);
					return ServiceResult<CashItemsDto>.Failure(
						new NotFoundException($"Cash item with id {cashItemId} not found."));
				}

				var cashItemDto = cashItem.Adapt<CashItemsDto>();

				return ServiceResult<CashItemsDto>.Success(cashItemDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching cash item with ID {CashItemId}: {Error}", cashItemId, ex);
				return ServiceResult<CashItemsDto>.Failure(
					new ServerErrorException("Could not fetch cash item."));
			}
		}
		#endregion

		#region Add CashItem to DB

		public async Task<ServiceResult<CashItemsDto>> AddCashItem(CashItemsDto c)
		{
			if (c == null) return ServiceResult<CashItemsDto>.Failure(
				new BadRequestException("Cash item data is required."));

			try
			{
				var cashItem = c.Adapt<tbl_CashItem>();

				await _context.AddAsync(cashItem);

				await _context.SaveChangesAsync();

				var outputDto = cashItem.Adapt<CashItemsDto>();

				return ServiceResult<CashItemsDto>.Success(outputDto);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while creating cash item: {Error}", ex);
				if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
				{
					string errorMessage = "The CashItem you are trying to create already exists in this system. Please choose another figure.";
					return ServiceResult<CashItemsDto>.Failure(new BadRequestException(errorMessage));
				}
				return ServiceResult<CashItemsDto>.Failure(
					new ServerErrorException("Could not create cash item."));
			}
		}
		#endregion

		#region Edit CashItem In DB
		public async Task<ServiceResult<CashItemsDto>> UpdateCashItem(string id, CashItemsDto c)
		{
			if (c == null) return ServiceResult<CashItemsDto>.Failure(
				new BadRequestException("Cash item data is required."));

			var cashItemInDb = await _context.tbl_CashItems.FirstOrDefaultAsync(x => x.Id == id);

			if (cashItemInDb == null) return ServiceResult<CashItemsDto>.Failure(
				new NotFoundException($"Cash item with ID:{id} does not exist."));

			if (c.Id != id) return ServiceResult<CashItemsDto>.Failure(
				   new BadRequestException($"Cash item with ID: {id} is not the same as cash item with ID: {c.Id}"));

			try
			{
				cashItemInDb.Amount = c.Amount;

				await _context.SaveChangesAsync();

				return ServiceResult<CashItemsDto>.Success(cashItemInDb.Adapt<CashItemsDto>());

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while updating cash item with ID {CashItemId}: {Error}", id, ex);
				if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
				{
					string errorMessage = "The CashItem you are trying to edit already exists in this system. Please choose another figure.";
					return ServiceResult<CashItemsDto>.Failure(new BadRequestException(errorMessage));
				}
				return ServiceResult<CashItemsDto>.Failure(
					new ServerErrorException("Could not update cash item."));
			}
		}
		#endregion

		#region Delete CashItem from DB
		public async Task<ServiceResult<bool>> DeleteCashItem(string id)
		{
			var cashItemInDb = await _context.tbl_CashItems.FindAsync(id);

			if (cashItemInDb == null) return ServiceResult<bool>.Failure(
				new NotFoundException($"Cash item with ID:{id} does not exist."));

			try
			{
				_context.tbl_CashItems.Remove(cashItemInDb);

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while deleting cash item with ID {CashItemId}: {Error}", id, ex);
				if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
				{
					string errorMessage = "The CashItem you are trying to delete already exists in this system. Please choose another figure.";
					return ServiceResult<bool>.Failure(new BadRequestException(errorMessage));
				}
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete cash item."));
			}

		}
		#endregion
	}
}
