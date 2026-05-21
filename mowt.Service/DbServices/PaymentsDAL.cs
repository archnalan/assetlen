using mowt.Service.DataAccess;
using mowt.Service.DbServices.ServiceInterfaces;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.Models.ViewModels.Users;
using mowt.Shared.Models.statics;
using mowt.Shared.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Service.DbServices
{
	public class PaymentsDAL : IPaymentsDAL
	{
		private readonly mowtDbContext _context;
		private readonly ILogger<PaymentsDAL> _logger;
		private readonly ITransactionDAL _transactionDAL;
		private readonly ITransactionDetailDAL _transactionDetailDAL;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IShiftsDAL _shiftsDAL;
		private readonly IConfigDAL _configDAL;


		public PaymentsDAL(ILogger<PaymentsDAL> logger, mowtDbContext context, ITransactionDAL transactionDAL, IHttpContextAccessor httpContextAccessor, IShiftsDAL shiftsDAL, ITransactionDetailDAL transactionDetailDAL, IConfigDAL configDAL)
		{
			_logger = logger;
			_context = context;
			_transactionDAL = transactionDAL;
			_httpContextAccessor = httpContextAccessor;
			_shiftsDAL = shiftsDAL;
			_transactionDetailDAL = transactionDetailDAL;
			_configDAL = configDAL;
		}


		#region Read Payments from Database
		public async Task<ServiceResult<List<PaymentsDto>>> GetPaymentsFromDB()
		{
			try
			{
				var payments = await _context.tbl_Payments.AsNoTracking().ToListAsync();

				var paymentsDto = payments.Adapt<List<PaymentsDto>>();

				return ServiceResult<List<PaymentsDto>>.Success(paymentsDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching payments from database: {Error}", ex);
				return ServiceResult<List<PaymentsDto>>.Failure(
					new ServerErrorException("Could not fetch payments."));
			}
		}
		#endregion

		#region Read Payments from Database based on SaleID
		public async Task<ServiceResult<List<PaymentsDto>>> GetPaymentsBasedOnSaleID(string saleId)
		{
			try
			{
				var payments = await _context.tbl_Payments.Include(x => x.PaymentMode).Where(c => c.SaleId == saleId).ToListAsync();

				return ServiceResult<List<PaymentsDto>>.Success(payments.Adapt<List<PaymentsDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching payments for sale ID {SaleId}: {Error}", saleId, ex);
				return ServiceResult<List<PaymentsDto>>.Failure(
					new ServerErrorException("Could not fetch payments for sale."));
			}
		}
		#endregion

		#region Read Sum of Payments from Database based on SaleID
		public async Task<ServiceResult<decimal>> GetSumOfPaymentsBasedOnSaleID(string saleID)
		{

			try
			{
				decimal sum = await _context.tbl_Payments
											.Where(c => c.SaleId == saleID && c.Amount != null)
											.SumAsync(s => (decimal)s.Amount);

				return ServiceResult<decimal>.Success(sum);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while calculating sum of payments for sale ID {SaleId}: {Error}", saleID, ex);
				return ServiceResult<decimal>.Failure(
					new ServerErrorException("Could not calculate sum of payments."));
			}

		}
		#endregion

		#region Add Payments to DB
		public async Task<ServiceResult<PaymentsDto>> AddPayments(PaymentsDto p)
		{
			if (p == null) return ServiceResult<PaymentsDto>.Failure(
				new BadRequestException("Payment data is required."));

			try
			{
				var payment = p.Adapt<tbl_Payment>();

				await _context.AddAsync(payment);

				await _context.SaveChangesAsync();

				var createdDto = payment.Adapt<PaymentsDto>();

				return ServiceResult<PaymentsDto>.Success(createdDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while creating payment with amount {Amount}: {Error}", p.Amount, ex);
				return ServiceResult<PaymentsDto>.Failure(
					new ServerErrorException("Could not create payment."));
			}
		}
		public async Task<ServiceResult<TransactionDto>> AddPaymentsAndCloseSale(List<PaymentsDto> p)
		{

			try
			{
				if (p == null)
					return ServiceResult<TransactionDto>.Failure(new BadRequestException("Payment data is required."));
				//TODO; 
				//validate payment amount ;


				// Create the execution strategy
				var executionStrategy = _context.Database.CreateExecutionStrategy();

				return await executionStrategy.ExecuteAsync(async () =>
				{
					var saleId = p.FirstOrDefault()?.SaleId;
					if (string.IsNullOrEmpty(saleId))
						return ServiceResult<TransactionDto>.Failure(
							new BadRequestException("Sale ID is required."));

					var transactionDetail = await _transactionDetailDAL
							.GetTransactionDetailBasedOnTransactionID(saleId);

					if (!transactionDetail.IsSuccess)
						return ServiceResult<TransactionDto>.Failure(transactionDetail.Error);

					if (transactionDetail.Data.Sum(x => x.CostInc) > p.Sum(x => x.Amount))
						return ServiceResult<TransactionDto>.Failure(new BadRequestException("Invalid Payment total"));

					var payment = p.Adapt<List<tbl_Payment>>();

					// Begin the transaction
					await using var transaction = await _context.Database.BeginTransactionAsync();

					try
					{
						// Complete the transaction
						var saleFromDb = await _context.tbl_Transactions
						.Include(x => x.TransactionDetails)
						.FirstOrDefaultAsync(x => x.Id == payment.First().SaleId);

						if (saleFromDb == null) return ServiceResult<TransactionDto>.Failure(
							new BadRequestException("Transaction you are paying for no longer exists"));

						if (saleFromDb.TransactionDetails != null)
						{
							int saleAmout = (int)p.Sum(x => x.Amount ?? 0);
							int totalPriceInc = (int)saleFromDb.TransactionDetails.Sum(x => x.TotalPriceInc ?? 0);

							if (saleAmout < totalPriceInc) return ServiceResult<TransactionDto>.Failure(
								new BadRequestException("Payment amount less than the expexted amount"));
						}

						// Add payments
						await _context.AddRangeAsync(payment);
						await _context.SaveChangesAsync();

						saleFromDb.TransactionDetails = null;

						if (saleFromDb.TransactionStatus == (int)statics.SaleStatus.OnHold)
						{
							saleFromDb.TransactionStatus = (int)statics.SaleStatus.ClosedFromHold;
						}
						else if (saleFromDb.TransactionStatus == (int)statics.SaleStatus.Order)
						{
							saleFromDb.TransactionStatus = (int)statics.SaleStatus.ClosedFromOrder;
						}
						else if (saleFromDb.TransactionStatus == (int)statics.SaleStatus.Quotation)
						{
							saleFromDb.TransactionStatus = (int)statics.SaleStatus.ClosedFromQuote;
						}
						else
						{
							saleFromDb.TransactionStatus = (int)statics.SaleStatus.ClosedInstantly;
						}

						saleFromDb.TransactionDate = DateTime.UtcNow;
						saleFromDb.SaleTotal = p.Sum(x => x.Amount);
						saleFromDb.SaleAgentId = null; // Set sale agent
						if (!string.IsNullOrEmpty(payment.FirstOrDefault()?.CustomerId ?? "")) saleFromDb.CustomerId = payment.FirstOrDefault()?.CustomerId ?? "";
						saleFromDb.Change = p.FirstOrDefault(x => x.PaymentModeId == "1")?.Change ?? 0; //check this later

						await _context.SaveChangesAsync();

						//update stock values
						var td = _context.tbl_TransactionDetails.Where(x => x.TransactionId == saleFromDb.Id).ToList();
						foreach (var item in td)
						{
							var stockItem = await _context.tbl_Products
								.FirstOrDefaultAsync(x => x.Id == item.ProductId);
							if (stockItem != null)
							{
								stockItem.InStock -= item.Qty;

								if (stockItem.InStock < 0)
								{
									var configResult = await _configDAL.GetSettingByID((int)statics.Configurations.PreventSellingOutOfStockItems);
									if (configResult.IsSuccess)
									{
										bool.TryParse(configResult.Data.StringValue, out bool outOfstock);
										if (outOfstock && stockItem.TrackInventory == true)
										{
											await transaction.RollbackAsync();
											_logger.LogError("Cannot complete payment with Id {ProductId} because the product is out of stock", stockItem.Id);
											return ServiceResult<TransactionDto>.Failure(
												new BadRequestException($"{stockItem.ProductName ?? "Unkown Product"} is out of stock"));
										}
									}
								}

							}
						}
						await _context.SaveChangesAsync();

						// Create a new transaction
						var identity = _httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
						var userClaims = identity?.Claims;
						var userJson = userClaims?.FirstOrDefault(x => x.Type.ToLower() == "user")?.Value;
						var userObj = JsonConvert.DeserializeObject<UserClaimsDto>(userJson ?? "");

						if (userObj == null || string.IsNullOrEmpty(userObj?.Id))
						{
							await transaction.RollbackAsync();
							_logger.LogError("Error while creating payment and closing sale: user not found.{UserObj}", userObj);
							return ServiceResult<TransactionDto>.Failure(
								new ServerErrorException("Could not create payment and close sale.No user found"));
						}

						var openShift = await _shiftsDAL.GetActiveShiftsforUserperUserId(userObj.Id);

						if (!openShift.IsSuccess)
						{
							await transaction.RollbackAsync();
							_logger.LogError("Error while creating payment and closing sale: {Error}", openShift.Error);
							return ServiceResult<TransactionDto>.Failure(openShift.Error);
						}
						var subActiveId = openShift.Data.SubActiveId;
						if (!string.IsNullOrEmpty(subActiveId))
						{
							var subSaleResult = await _transactionDAL.GetTransactionFromDB(openShift.Data.SubActiveId);
							if (!subSaleResult.IsSuccess)
							{
								await transaction.RollbackAsync();
								return ServiceResult<TransactionDto>.Failure(subSaleResult.Error);
							}
							// Check if the sub sale is still open
							if (subSaleResult.Data.TransactionStatus == null
								|| (subSaleResult.Data.TransactionStatus.HasValue
								&& subSaleResult.Data.TransactionStatus.Value
								< (int)statics.SaleStatus.ClosedInstantly))
							{
								openShift.Data.ActiveId = subSaleResult.Data.Id;
								openShift.Data.SubActiveId = null;

								var shiftUpdate = await _shiftsDAL.UpdateShiftsUsingShiftId(openShift.Data);
								if (!shiftUpdate.IsSuccess)
								{
									await transaction.RollbackAsync();
									_logger.LogError("Error while creating payment and closing sale: {Error}", shiftUpdate.Error);
									return ServiceResult<TransactionDto>.Failure(shiftUpdate.Error);
								}

								await transaction.CommitAsync();
								return ServiceResult<TransactionDto>.Success(subSaleResult.Data);

							}
						}
						var t = new TransactionDto
						{
							SoldBy = userObj?.Id,
							ShiftId = openShift.Data.Id,
							TransactionStatus = (int)statics.SaleStatus.opened
						};

						var y = await _transactionDAL.CreateNewTransaction(t);

						if (!y.IsSuccess)
						{
							await transaction.RollbackAsync();
							return ServiceResult<TransactionDto>.Failure(y.Error);
						}
						// Update shift active ID
						openShift.Data.ActiveId = y.Data.Id;
						var z = await _shiftsDAL.UpdateShiftsUsingShiftId(openShift.Data);

						if (!z.IsSuccess)
						{
							await transaction.RollbackAsync();
							_logger.LogError("Error while creating payment and closing sale: {Error}", z.Error);
							return ServiceResult<TransactionDto>.Failure(z.Error);
						}

						await transaction.CommitAsync();
						return ServiceResult<TransactionDto>.Success(y.Data);
					}
					catch (Exception ex)
					{
						// Rollback on exception
						await transaction.RollbackAsync();
						_logger.LogError("Error while creating payment and closing sale: {Error}", ex);
						throw; // Re-throw to allow retries
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while creating payment and closing sale: {Error}", ex);
				return ServiceResult<TransactionDto>.Failure(
					new ServerErrorException("Could not create payment and close sale."));
			}
		}
		#endregion

		#region Delete Payment Hard
		public async Task<ServiceResult<bool>> DeletePayment(string id)
		{
			var pricingInDb = await _context.tbl_Payments
								.FirstOrDefaultAsync(p => p.Id == id);


			if (pricingInDb == null) return ServiceResult<bool>
					.Failure(new NotFoundException($"Payment with ID: {id} not found."));

			try
			{
				//hard delete
				_context.tbl_Payments.Remove(pricingInDb);

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while deleting payment with ID {PaymentId}: {Error}", id, ex);
				return ServiceResult<bool>.Failure(
					new ServerErrorException("Could not delete payment."));
			}
		}
		#endregion

		#region Read PaymentMode Name from Database based on PaymentModeID
		public async Task<ServiceResult<string>> GetPaymentModeNameUsingID(string id)
		{
			try
			{
				var paymentModeInDb = await _context.tbl_PaymentModes.FindAsync(id);

				if (paymentModeInDb == null) return ServiceResult<string>
					.Failure(new NotFoundException($"Payment mode with ID: {id} not found."));

				if (!string.IsNullOrEmpty(paymentModeInDb.Description))
				{
					string paymentModeName = paymentModeInDb.Description.ToString();

					return ServiceResult<string>.Success(paymentModeName);
				}
				else
				{
					_logger.LogError("Payment mode with ID {PaymentModeId} does not have a description", id);
					return ServiceResult<string>.Failure(
						new ServerErrorException("Payment mode does not have a description."));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching payment mode with ID {PaymentModeId}: {Error}", id, ex);
				return ServiceResult<string>.Failure(
					new ServerErrorException("Could not fetch payment mode."));
			}

		}
		#endregion

		#region Read ALL PaymentMode Name from Database
		public async Task<ServiceResult<List<PaymentModeDto>>> GetAllPaymentModes()
		{
			try
			{
				var paymentModes = await _context.tbl_PaymentModes
											.AsNoTracking()
											.OrderBy(c => c.Description)
											.ToListAsync();

				var paymentModesDto = paymentModes.Adapt<List<PaymentModeDto>>();

				return ServiceResult<List<PaymentModeDto>>.Success(paymentModesDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching payment modes from database: {Error}", ex);
				return ServiceResult<List<PaymentModeDto>>.Failure(
					new ServerErrorException("Could not fetch payment modes."));
			}

		}
		#endregion

		#region Read PaymentAccounts from Database		
		public async Task<ServiceResult<List<PaymentAccountDto>>> GetCARDPaymentAccountFromDB()
		{
			try
			{
				var paymentModes = await _context.tbl_PaymentAccounts
											.AsNoTracking()
											.Where(p => p.PaymentTypeId == 3) // Card payment type id is 3
											.OrderBy(c => c.PaymentAccountName)
											.ToListAsync();

				var paymentModesDto = paymentModes.Adapt<List<PaymentAccountDto>>();

				return ServiceResult<List<PaymentAccountDto>>.Success(paymentModesDto);
			}
			catch (Exception ex)
			{

				_logger.LogError("Error while fetching card payment accounts from database: {Error}", ex);
				return ServiceResult<List<PaymentAccountDto>>.Failure(
					new ServerErrorException("Could not fetch card payment accounts."));
			}
		}
		#endregion

		#region Read PaymentAccounts from Database		
		public async Task<ServiceResult<List<PaymentAccountDto>>> SearchCARDPaymentAccountFromDBUsingKeyword(string keywords)
		{
			try
			{
				if (string.IsNullOrEmpty(keywords))
				{
					var accountResult = await _context.tbl_PaymentAccounts
												.Where(a => a.PaymentTypeId == 3) // card payment type id is 3
												.ToListAsync();

					var accountResultDto = accountResult.Adapt<List<PaymentAccountDto>>();

					return ServiceResult<List<PaymentAccountDto>>.Success(accountResultDto);
				}


				var paymentAccounts = await _context.tbl_PaymentAccounts
												.Where(c => !string.IsNullOrEmpty(c.PaymentAccountName)
												&& c.PaymentAccountName.Contains(keywords))
												.ToListAsync();

				var paymentAccountsDto = paymentAccounts.Adapt<List<PaymentAccountDto>>();

				return ServiceResult<List<PaymentAccountDto>>.Success(paymentAccountsDto);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching card payment accounts with keywords '{Keywords}': {Error}", keywords, ex);
				return ServiceResult<List<PaymentAccountDto>>.Failure(
					new ServerErrorException("Could not search card payment accounts."));
			}
		}
		#endregion

		#region Read PaymentAccounts from Database		
		public async Task<ServiceResult<List<PaymentAccountDto>>> GetBANKPaymentAccountFromDB()
		{
			try
			{
				var paymentModes = await _context.tbl_PaymentAccounts
											.AsNoTracking()
											.Where(p => p.PaymentTypeId == 5) //Bank accounts are of payment type id 5
											.OrderBy(c => c.PaymentAccountName)
											.ToListAsync();

				var paymentModesDto = paymentModes.Adapt<List<PaymentAccountDto>>();

				return ServiceResult<List<PaymentAccountDto>>.Success(paymentModesDto);
			}
			catch (Exception ex)
			{

				_logger.LogError("Error while fetching bank payment accounts from database: {Error}", ex);
				return ServiceResult<List<PaymentAccountDto>>.Failure(
					new ServerErrorException("Could not fetch bank payment accounts."));
			}
		}
		#endregion

		#region Read Bank Accounts from Database
		public async Task<ServiceResult<List<PaymentAccountDto>>> SearchBANKAccountFromDBUsingKeyword(string keywords)
		{
			try
			{
				if (string.IsNullOrEmpty(keywords))
				{
					var accountResult = await _context.tbl_PaymentAccounts
												.Where(a => a.PaymentTypeId == 5) // bank payment type id is 5
												.ToListAsync();

					var accountResultDto = accountResult.Adapt<List<PaymentAccountDto>>();

					return ServiceResult<List<PaymentAccountDto>>.Success(accountResultDto);
				}


				var paymentAccounts = await _context.tbl_PaymentAccounts
												.Where(c => !string.IsNullOrEmpty(c.PaymentAccountName)
												&& c.PaymentAccountName.Contains(keywords))
												.ToListAsync();

				var paymentAccountsDto = paymentAccounts.Adapt<List<PaymentAccountDto>>();

				return ServiceResult<List<PaymentAccountDto>>.Success(paymentAccountsDto);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error while searching bank payment accounts with keywords '{Keywords}': {Error}", keywords, ex);
				return ServiceResult<List<PaymentAccountDto>>.Failure(
					new ServerErrorException("Could not search bank payment accounts."));
			}
		}
		#endregion

	}
}
