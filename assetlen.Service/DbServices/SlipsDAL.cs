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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices
{
	public class SlipsDAL : ISlipsDAL
	{
		private readonly AssetlenDbContext _context;
		private readonly ILogger<SlipsDAL> _logger;

		public SlipsDAL(ILogger<SlipsDAL> logger, AssetlenDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		#region Read Slips from Database 
		public async Task<ServiceResult<SizeDto>> GetSlipdetailsFromDBbasedOnslipID(string sizeId)
		{
			try
			{
				var result = await _context.tbl_Sizes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sizeId);

				if (result == null)
				{
					_logger.LogError("Slip with ID: {SlipId} not found.", sizeId);
					return ServiceResult<SizeDto>.Failure(
						new NotFoundException($"Slip with ID: {sizeId} not found."));
				}

				return ServiceResult<SizeDto>.Success(result.Adapt<SizeDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching slip with ID {SlipId}: {Error}", sizeId, ex);
				return ServiceResult<SizeDto>.Failure(
					new ServerErrorException("Could not fetch slip details."));
			}
		}
		#endregion

		#region Read ALL Slips from Database 
		public async Task<ServiceResult<List<SizeDto>>> GetAllSlipdetailsFromDB()
		{
			try
			{
				var result = await _context.tbl_Sizes.AsNoTracking().ToListAsync();
				return ServiceResult<List<SizeDto>>.Success(result.Adapt<List<SizeDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching all slips: {Error}", ex);
				return ServiceResult<List<SizeDto>>.Failure(
					new ServerErrorException("Could not fetch slips."));
			}
		}
		#endregion

		#region update Slips  in the  DB

		public async Task<ServiceResult<SizeDto>> UpdateOrCreateSlipsUsingSlipID(SizeDto sizeDto)
		{
			if (sizeDto == null)
				return ServiceResult<SizeDto>.Failure(new BadRequestException("Slip data is required."));

			try
			{
				var size = sizeDto.Adapt<tbl_Size>();
				var objFromDb = await _context.tbl_Sizes.FirstOrDefaultAsync(x => x.Id == size.Id);

				if (objFromDb == null && !string.IsNullOrEmpty(sizeDto.Id))
				{
					_context.Add(size);
					await _context.SaveChangesAsync();
					return ServiceResult<SizeDto>.Success(size.Adapt<SizeDto>());
				}
				else if (objFromDb != null)
				{
					objFromDb.Width = size.Width ?? objFromDb.Width;
					objFromDb.Height = size.Height ?? objFromDb.Height;
					await _context.SaveChangesAsync();
					return ServiceResult<SizeDto>.Success(objFromDb.Adapt<SizeDto>());
				}

				_logger.LogError("Slip with ID: {SlipId} not found for update or creation.", size.Id);
				return ServiceResult<SizeDto>.Failure(
					new NotFoundException($"Slip with ID: {size.Id} not found."));
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while updating or creating slip with ID {SlipId}: {Error}", sizeDto?.Id, ex);
				return ServiceResult<SizeDto>.Failure(
					new ServerErrorException("Could not update or create slip."));
			}
		}
		#endregion
	}
}
