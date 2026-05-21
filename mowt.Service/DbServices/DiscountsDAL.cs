using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using mowt.Service.Extensions;
using Mapster;

namespace mowt.Service.DbServices
{
    public class DiscountsDAL : IDiscountsDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<CategoryDAL> _logger;
        private readonly ITenantProvider _tenantProvider;

        public DiscountsDAL(ILogger<CategoryDAL> logger, mowtDbContext context, ITenantProvider tenantProvider)
        {
            _logger = logger;
            _context = context;
            _tenantProvider = tenantProvider;
        }

        #region Read Discounts from Database
        public async Task<ServiceResult<PaginationDetails<DiscountDto>>> GetDiscountsFromDB(int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var discounts = await _context.tbl_Discounts.AsNoTracking().OrderBy(c => c.DiscountValue).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var discountsDto = discounts.Adapt<PaginationDetails<DiscountDto>>();

                return ServiceResult<PaginationDetails<DiscountDto>>.Success(discountsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching discounts from database: {Error}", ex);
                return ServiceResult<PaginationDetails<DiscountDto>>.Failure(
                    new ServerErrorException("Could not fetch discounts."));
            }
        }
        #endregion

        #region Get Discount from Database based on DiscountID
        public async Task<ServiceResult<DiscountDto>> GetDiscountById(string id)
        {
            try
            {
                var discount = await _context.tbl_Discounts.FindAsync(id);

                if (discount == null)
                {
                    _logger.LogError("Discount with ID: {DiscountId} not found.", id);
                    return ServiceResult<DiscountDto>.Failure(
                        new NotFoundException($"Discount with ID: {id} not found."));
                }

                return ServiceResult<DiscountDto>.Success(discount.Adapt<DiscountDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching discount with ID {DiscountId}: {Error}", id, ex);
                return ServiceResult<DiscountDto>.Failure(
                    new ServerErrorException("Could not fetch discount."));
            }
        }
        #endregion

        #region Read Discounts from Database for ComboBoxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> GetDiscountsFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var discounts = await _context.tbl_Discounts.AsNoTracking()
                    .Where(x => x.Id.ToString().Contains(keywords) ||
                    (x.DiscountValue ?? 0).ToString().ToLower().Contains(keywords.ToLower()))
                    .Select(x => new ComboBoxDto
                    {
                        Id = x.Id,
                        IdString = x.Id.ToString(),
                        ValueText = (x.DiscountValue ?? 0).ToString(),
                        Selected = (x.Active ?? true)
                    })
                    .ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(discounts);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching discounts for combobox: {Error}", ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException("Could not fetch discounts for combobox."));
            }
        }
        #endregion

        #region Search Discounts from Database for ComboBoxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchDiscountsFromComboBoxes(string keywords, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending, bool isActive)
        {
            try
            {
                var discounts = await _context.tbl_Discounts.AsNoTracking()
                    .Where(x => x.Active == isActive && (x.Id.ToString().Contains(keywords) ||
                    (x.DiscountValue ?? 0).ToString().ToLower().Contains(keywords.ToLower())))
                    .Select(x => new ComboBoxDto
                    {
                        Id = x.Id,
                        IdString = x.Id.ToString(),
                        ValueText = (x.DiscountValue ?? 0).ToString(),
                        Selected = (x.Active ?? true)
                    })
                    .ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(discounts);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching discounts for combobox with keywords '{Keywords}': {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException("Could not search discounts for combobox."));
            }
        }
        #endregion

        #region Add Discount to DB
        public async Task<ServiceResult<DiscountDto>> AddDiscount(DiscountCreateDto d)
        {
            if (d == null) return ServiceResult<DiscountDto>.Failure(
                                new BadRequestException("Discount data is required."));

            try
            {
                var disc = d.Adapt<tbl_Discount>();

                await _context.AddAsync(disc);

                await _context.SaveChangesAsync();

                var createdDisc = disc.Adapt<DiscountDto>();

                return ServiceResult<DiscountDto>.Success(createdDisc);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating discount: {Error}", ex);
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
                {
                    string ErrorMessage = "The discount you are trying to create already exists in this system. Please choose another value.";
                    return ServiceResult<DiscountDto>.Failure(new BadRequestException(ErrorMessage));
                }

                return ServiceResult<DiscountDto>.Failure(
                    new ServerErrorException("Could not create discount."));
            }
        }
        #endregion

        #region Get Discount from Database based on DiscountValue
        public async Task<ServiceResult<DiscountDto>> GetDiscountByValue(decimal value)
        {
            try
            {
                var discount = await _context.tbl_Discounts.FirstOrDefaultAsync(c => c.DiscountValue == value);

                if (discount == null)
                {
                    _logger.LogError("Discount with value: {DiscountValue} not found.", value);
                    return ServiceResult<DiscountDto>.Failure(
                        new NotFoundException($"Discount with value: {value} not found."));
                }

                return ServiceResult<DiscountDto>.Success(discount.Adapt<DiscountDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching discount with value {DiscountValue}: {Error}", value, ex);
                return ServiceResult<DiscountDto>.Failure(
                    new ServerErrorException("Could not fetch discount."));
            }
        }
        #endregion

        #region Update Discount in the DB
        public async Task<ServiceResult<DiscountDto>> UpdateDiscount(string id, DiscountDto dDto)
        {
            if (dDto == null) return ServiceResult<DiscountDto>.Failure(
                                new BadRequestException("Discount data is required."));

            if (dDto.Id != id) return ServiceResult<DiscountDto>.Failure(
                    new BadRequestException($"Discount with ID: {id} is not the same as discount with ID: {dDto.Id}"));

            var discountInDb = await _context.tbl_Discounts.FirstOrDefaultAsync(c => c.Id == id);

            if (discountInDb == null) return ServiceResult<DiscountDto>.Failure(
                                    new NotFoundException($"Discount with ID {id} not found."));

            try
            {
                //Map the incoming data excluding unchanged properties
                discountInDb.DiscountValue = dDto.DiscountValue ?? discountInDb.DiscountValue;
                discountInDb.Active = dDto.Active ?? discountInDb.Active;

                await _context.SaveChangesAsync();

                return ServiceResult<DiscountDto>.Success(discountInDb.Adapt<DiscountDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating discount with ID {DiscountId}: {Error}", id, ex);
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint"))
                {
                    string ErrorMessage = "The discount you are trying to update already exists in this system. Please choose another value.";
                    return ServiceResult<DiscountDto>.Failure(new BadRequestException(ErrorMessage));
                }

                return ServiceResult<DiscountDto>.Failure(
                    new ServerErrorException("Could not update discount."));
            }
        }
        #endregion

        #region Delete Discount from DB
        public async Task<ServiceResult<bool>> DeleteDiscountById(string id)
        {
            var discountInDb = await _context.tbl_Categories.FindAsync(id);

            if (discountInDb == null) return ServiceResult<bool>
                    .Failure(new NotFoundException($"Discount with ID: {id} not found."));

            try
            {
                //soft delete
                discountInDb.IsDeleted = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting discount with ID {DiscountId}: {Error}", id, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete discount."));
            }
        }
        #endregion
    }
}
