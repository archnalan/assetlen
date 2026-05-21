using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ProductStructureDtos;
using assetlen.Shared.Models.statics;
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
	public class ReceiptsDAL : IReceiptsDAL
	{
		private readonly AssetlenDbContext _context;
		private readonly ILogger<ReceiptsDAL> _logger;
		private readonly ITenantProvider _tenantProvider;

		public ReceiptsDAL(ILogger<ReceiptsDAL> logger, AssetlenDbContext context, ITenantProvider tenantProvider)
		{
			_logger = logger;
			_context = context;
			_tenantProvider = tenantProvider;
		}

		#region Read ReceiptItems from Database BasedOn SlipID
		public async Task<ServiceResult<List<ReceiptItemDto>>> GetReceiptItemsFromDBbasedOnSlipID(int slipId)
		{
			try
			{
				var receipts = await _context.tbl_SlipLayouts.Where(c => c.SlipID == slipId).ToListAsync();

				return ServiceResult<List<ReceiptItemDto>>.Success(receipts.Adapt<List<ReceiptItemDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching receipts in db {error}", ex);
				return ServiceResult<List<ReceiptItemDto>>.Failure(
					new ServerErrorException("Could not fetch report"));
			}
		}
		#endregion

		//#region Read ReceiptItems from Database BasedOn SlipID and PrintItemID
		//public async Task<ServiceResult<List<ReceiptDto>>> TableDescriptionColumnExists(int slipId, int printItemId)
		//{
		//	try
		//	{
		//		var receipts = await _context.tbl_SlipLayouts
		//						.Where(s => s.SlipId == slipId && s.PrintItemId == printItemId)
		//						.OrderBy(s => s.Y).ToListAsync();

		//		return ServiceResult<List<ReceiptDto>>.Success(receipts.Adapt<List<ReceiptDto>>());
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError("Error fetching receipts in db {error}", ex);
		//		return ServiceResult<List<ReceiptDto>>.Failure(
		//			new ServerErrorException($"Error fetching receipts: {ex.Message}"));
		//	}
		//}
		//#endregion

		//#region Read ReceiptItems from Database on PrintItem ID
		//public async Task<ServiceResult<List<ReceiptDto>>> GetReceiptItemsFromDBbasedOnPrintItemID(int printItemId)
		//{
		//	try
		//	{
		//		var receipts = await _context.tbl_SlipLayouts
		//						.Where(s => s.PrintItemId == printItemId)
		//						.ToListAsync();

		//		return ServiceResult<List<ReceiptDto>>.Success(receipts.Adapt<List<ReceiptDto>>());
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError("Error fetching receipts in db {error}", ex);
		//		return ServiceResult<List<ReceiptDto>>.Failure(
		//			new ServerErrorException($"Error fetching receipts: {ex.Message}"));
		//	}
		//}
		//#endregion

		//#region Add ReceiptItems to DB
		//public async Task<ServiceResult<ReceiptDto>> AddReceiptItemsWithSlipID(ReceiptDto receiptDto)
		//{

		//	if (receiptDto == null) return ServiceResult<ReceiptDto>.Failure(
		//		new BadRequestException("Slip Layout data is required."));

		//	try
		//	{
		//		var receipt = receiptDto.Adapt<tbl_SlipLayout>();

		//		await _context.AddAsync(receipt);

		//		await _context.SaveChangesAsync();

		//		return ServiceResult<ReceiptDto>.Success(receipt.Adapt<ReceiptDto>());
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogError($"Receipt with slip ID {receiptDto.SlipId} could not be created.", ex);
		//		return ServiceResult<ReceiptDto>.Failure(
		//			new ServerErrorException($"Receipt with slip ID {receiptDto.SlipId} could not be created: {ex.Message}"));
		//	}
		//}
		//      #endregion
		#region AddOrUpdate ReceiptItems to DB

		public async Task<ServiceResult<List<ReceiptItemDto>>> CreateOrSyncNewReceiptItems(List<ReceiptItemDto> rDto)
		{
			//allow clearing list
			//if (tdDto.Count==0) return ServiceResult<List<TransactionDetailDto>>.Failure(
			//							   new BadRequestException("Transaction Detail data is required."));


			//TODO: run this as a transaction transadetail table too
			try
			{

				var itemsAlreadyInDb = _context.tbl_SlipLayouts.AsNoTracking().Where(x => x.SlipID == rDto.First().SlipID);

				var receiptsEntityList = rDto.Adapt<List<tbl_SlipLayout>>();
				if (itemsAlreadyInDb.Count() == 0)
				{
					//first time syncing receipts

					await _context.tbl_SlipLayouts.AddRangeAsync(receiptsEntityList);
					await _context.SaveChangesAsync();

				}
				else
				{
					//items that already exist in the db
					foreach (var item in receiptsEntityList)
					{

						if (string.IsNullOrEmpty(item.Id))
						{
							//items that were added to list

							await _context.tbl_SlipLayouts.AddAsync(item);
						}
						else
						{
							//items that were updated
							item.TenantId = _tenantProvider.GetTenantId();
							_context.tbl_SlipLayouts.Update(item);
						}

					}
					//if it was deleted
					var incomingOldIds = receiptsEntityList.Where(x => !string.IsNullOrEmpty(x.Id)).Select(x => x.Id);

					var deletedItems = itemsAlreadyInDb.Where(x => !(incomingOldIds.Contains(x.Id))).ToList();

					if (deletedItems.Count() > 0) _context.tbl_SlipLayouts.RemoveRange(deletedItems);
					//commiting changes
					await _context.SaveChangesAsync();


				}

				var createdDto = itemsAlreadyInDb.ToList().Adapt<List<ReceiptItemDto>>();

				return ServiceResult<List<ReceiptItemDto>>.Success(createdDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while creating Receipt detail: {error}", ex);
				return ServiceResult<List<ReceiptItemDto>>.Failure(new ServerErrorException("Could not create receipt slip detail."));
			}
		}
		#endregion




	}
}
