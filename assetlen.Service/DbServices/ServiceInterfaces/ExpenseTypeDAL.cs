using assetlen.Service.DataAccess;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public class ExpenseTypeDAL : IExpenseTypeDAL
    {
        private readonly AssetlenDbContext _context;

        private readonly ILogger<ExpenseDAL> _logger;

        public ExpenseTypeDAL(AssetlenDbContext context, ILogger<ExpenseDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<List<ExpenseTypeDto>>> GetExpenseTypes()
        {
            try
            {
                var expenseTypes = _context.tbl_ExpenseTypes.ToList();
                var expenseTypeDtos = expenseTypes.Select(x => x.Adapt<ExpenseTypeDto>()).ToList();
                return ServiceResult<List<ExpenseTypeDto>>.Success(expenseTypeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting expense types: {ex.Message}", ex);
                return ServiceResult<List<ExpenseTypeDto>>.Failure(new ServerErrorException($"Error while getting expense types: {ex.Message}"));
            }
        }
        public async Task<ServiceResult<List<ComboBoxDto>>> GetExpensesForComboBox()
        {
            try
            {
                var expenseTypes = await _context.tbl_ExpenseTypes.ToListAsync();
                var comboBoxDtos = expenseTypes.Select(x => new ComboBoxDto
                {
                    Id = x.Id,
                    ValueText = x.Description ?? "",
                    IdString = x.Id.ToString()
                }).ToList();
                return ServiceResult<List<ComboBoxDto>>.Success(comboBoxDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting expense types for combo box: {ex.Message}", ex);
                return ServiceResult<List<ComboBoxDto>>.Failure(new ServerErrorException($"Error while getting expense types for combo box: {ex.Message}"));
            }
        }

        public async Task<ServiceResult<ExpenseTypeDto>> GetExpenseType([Required] string typeId)
        {
            try
            {
                var expenseType = await _context.tbl_ExpenseTypes.FirstOrDefaultAsync(x => x.Id == typeId);
                if (expenseType == null)
                {
                    return ServiceResult<ExpenseTypeDto>.Failure(new NotFoundException($"Expense with ID:{typeId} type not found."));
                }
                var expenseTypeDto = expenseType.Adapt<ExpenseTypeDto>();
                return ServiceResult<ExpenseTypeDto>.Success(expenseTypeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting expense type: {ex.Message}", ex);
                return ServiceResult<ExpenseTypeDto>.Failure(new ServerErrorException($"Error while getting expense type: {ex.Message}"));
            }
        }

        public async Task<ServiceResult<ExpenseTypeDto>> AddExpenseType([Required] ExpenseTypeDto expenseTypeDto)
        {
            try
            {
                var expenseType = expenseTypeDto.Adapt<tbl_ExpenseType>();
                _context.tbl_ExpenseTypes.Add(expenseType);
                await _context.SaveChangesAsync();
                var createdExpenseTypeDto = expenseType.Adapt<ExpenseTypeDto>();
                return ServiceResult<ExpenseTypeDto>.Success(createdExpenseTypeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating expense type: {ex.Message}", ex);
                return ServiceResult<ExpenseTypeDto>.Failure(new ServerErrorException($"Error while creating expense type: {ex.Message}"));
            }
        }

        public async Task<ServiceResult<List<ExpenseTypeDto>>> AddMultipleExpenseTypes([Required] List<ExpenseTypeDto> expenseTypeDtoList)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var expenseTypes = expenseTypeDtoList.Adapt<List<tbl_ExpenseType>>();
                        foreach (var expenseType in expenseTypes)
                        {
                            expenseType.DateTimeCreated = DateTime.UtcNow;
                        }
                        _context.tbl_ExpenseTypes.AddRange(expenseTypes);
                        await _context.SaveChangesAsync();
                        var createdExpenseTypeDtoList = expenseTypes.Adapt<List<ExpenseTypeDto>>();
                        await transaction.CommitAsync();
                        return ServiceResult<List<ExpenseTypeDto>>.Success(createdExpenseTypeDtoList);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Error while creating expense types: {ex.Message}", ex);
                        return ServiceResult<List<ExpenseTypeDto>>.Failure(new ServerErrorException($"Error while creating expense types: {ex.Message}"));
                    }
                }
            });
        }

        public async Task<ServiceResult<ExpenseTypeDto>> UpdateExpenseType([Required] ExpenseTypeDto expenseTypeDto)
        {
            try
            {
                var expenseType = await _context.tbl_ExpenseTypes.FirstOrDefaultAsync(x => x.Id == expenseTypeDto.Id);
                if (expenseType == null)
                {
                    return ServiceResult<ExpenseTypeDto>.Failure(new NotFoundException($"Expense with ID:{expenseTypeDto.Id} type not found."));
                }
                expenseType.Description = expenseTypeDto.Description;
                expenseType.DateTimeModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                var updatedExpenseTypeDto = expenseType.Adapt<ExpenseTypeDto>();
                return ServiceResult<ExpenseTypeDto>.Success(updatedExpenseTypeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating expense type: {ex.Message}", ex);
                return ServiceResult<ExpenseTypeDto>.Failure(new ServerErrorException($"Error while updating expense type: {ex.Message}"));
            }
        }

        public async Task<ServiceResult<bool>> DeleteExpenseType([Required] string typeId)
        {
            try
            {
                var expenseType = await _context.tbl_ExpenseTypes.FirstOrDefaultAsync(x => x.Id == typeId);
                if (expenseType == null)
                {
                    return ServiceResult<bool>.Failure(new NotFoundException($"Expense with ID:{typeId} type not found."));
                }
                _context.tbl_ExpenseTypes.Remove(expenseType);
                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting expense type: {ex.Message}", ex);
                return ServiceResult<bool>.Failure(new ServerErrorException($"Error while deleting expense type: {ex.Message}"));
            }
        }

        public async Task<ServiceResult<List<ComboBoxDto>>> SearchExpenseTypesForComboBoxes(string? searchText)
        {
            try
            {
                var expenseTypesQuery = _context.tbl_ExpenseTypes
                    .Where(x => x.Description != null);

                if (!string.IsNullOrEmpty(searchText))
                {
                    expenseTypesQuery = expenseTypesQuery
                        .Where(x => EF.Functions.Like(x.Description, $"%{searchText}%"));
                }

                var expenseTypes = await expenseTypesQuery.ToListAsync();

                var comboBoxDtos = expenseTypes.Select(x => new ComboBoxDto
                {
                    Id = x.Id,
                    ValueText = x.Description ?? "",
                    IdString = x.Id.ToString()
                }).ToList();

                return ServiceResult<List<ComboBoxDto>>.Success(comboBoxDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching expense types for combo box: {ex.Message}", ex);
                return ServiceResult<List<ComboBoxDto>>.Failure(new ServerErrorException($"Error while searching expense types for combo box: {ex.Message}"));
            }
        }

    }
}
