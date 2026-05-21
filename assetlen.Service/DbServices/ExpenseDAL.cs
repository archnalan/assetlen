using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
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
	public class ExpenseDAL : IExpenseDAL
	{
		private readonly mowtDbContext _context;

		private readonly ILogger<ExpenseDAL> _logger;
		public ExpenseDAL(mowtDbContext context, ILogger<ExpenseDAL> logger)
		{
			_context = context;
			_logger = logger;
		}

		#region Create New Expense
		public async Task<ServiceResult<ExpenseDto>> CreateExpense(ExpenseDto expenseDto)
		{
			if (expenseDto == null) return ServiceResult<ExpenseDto>.Failure(
										   new BadRequestException("Expense data is required."));
			var strategy = _context.Database.CreateExecutionStrategy();

			return await strategy.ExecuteAsync(async () =>
			{
				using (var transaction = _context.Database.BeginTransaction())
				{
					try
					{
						var expense = expenseDto.Adapt<tbl_Expense>();

						expense.DateTimePayed = DateTime.UtcNow;

						_context.tbl_Expenses.Add(expense);

						await _context.SaveChangesAsync();

						var payments = expenseDto.ExpensePayments.Adapt<List<tbl_Payment>>();

						foreach (var payment in payments)
						{
							payment.ExpenseId = expense.Id;
						}

						await _context.tbl_Payments.AddRangeAsync(payments);

						await _context.SaveChangesAsync();

						var createdExpenseDto = expense.Adapt<ExpenseDto>();
						var createdPaymentsDto = payments.Adapt<List<PaymentsDto>>();

						foreach (var payment in createdPaymentsDto) payment.ExpenseId = createdExpenseDto.Id;

						createdExpenseDto.ExpensePayments = createdPaymentsDto;

						await transaction.CommitAsync();
						return ServiceResult<ExpenseDto>.Success(createdExpenseDto);
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						_logger.LogError("Error while creating expense: {Error}", ex);
						return ServiceResult<ExpenseDto>.Failure(
							new ServerErrorException("Could not create expense."));
					}
				}
			});


		}

		#endregion

		#region search expense for each shift and payment mode
		public async Task<ServiceResult<List<ExpensePerShiftDto>>> SearchExpensePerShiftAndPaymentModeID(string shiftID, int paymentModeID)
		{

			try
			{

				string sql = @"Select
                              tbl_Payments.Id as PaymentID,
                              tbl_Payments.PaymentModeID,
                              tbl_Expense.shiftID,
                              tbl_Payments.Amount
                            From
                              tbl_Payments Inner Join
                              tbl_Expense On tbl_Payments.ExpenseID = tbl_Expense.Id Inner Join
                              tbl_shifts On tbl_Expense.shiftID = tbl_shifts.Id
                            Where
                              tbl_Payments.PaymentModeID = @PaymentModeID And
                              tbl_Expense.shiftID = @ShiftID";

				var parameters = new[]
				{
					new SqlParameter("@PaymentModeID", paymentModeID),
					new SqlParameter("@ShiftID", shiftID)
				};

				var result = await _context.Database.SqlQueryRaw<ExpensePerShiftDto>(sql, parameters).ToListAsync();


				return ServiceResult<List<ExpensePerShiftDto>>.Success(result);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching expenses for shiftID {ShiftID} and paymentModeID {PaymentModeID}: {Error}", shiftID, paymentModeID, ex);
				return ServiceResult<List<ExpensePerShiftDto>>.Failure(
					new ServerErrorException("Could not search expenses."));
			}
		}
		#endregion
	}
}
