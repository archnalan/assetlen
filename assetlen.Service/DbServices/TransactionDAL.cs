using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.Service.FileProcessingServices;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.Models.ViewModels.Users;
using assetlen.Shared.Models.statics;
using assetlen.Shared.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using static assetlen.Shared.Models.statics.statics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace assetlen.Service.DbServices
{
	public class TransactionDAL : ITransactionDAL
	{
		private readonly AssetlenDbContext _context;
		private readonly ILogger<TransactionDAL> _logger;
		private IExcelDomainService _excelDomainService;
		public TransactionDAL(ILogger<TransactionDAL> logger, AssetlenDbContext context, IExcelDomainService excelDomainService)
		{
			_logger = logger;
			_context = context;
			_excelDomainService = excelDomainService;
		}

		#region Create New Transaction

		public async Task<ServiceResult<TransactionDto>> CreateNewTransaction(TransactionDto transactDto)
		{
			if (transactDto == null) return ServiceResult<TransactionDto>.Failure(
				new BadRequestException("Transaction data is required"));

			try
			{
				var transaction = transactDto.Adapt<tbl_Transaction>();

				await _context.tbl_Transactions.AddAsync(transaction);

				await _context.SaveChangesAsync();

				var createdConfigDto = transaction.Adapt<TransactionDto>();

				return ServiceResult<TransactionDto>.Success(createdConfigDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Transaction {TransactionStatus} could not be created.{Error}", transactDto.TransactionStatus, ex);
				if (ex.Message.Contains("Violation of UNIQUE KEY constraint"))
				{
					string ErrorMessage = "The transaction you are trying to add already exists in this system. Please choose another number";
					return ServiceResult<TransactionDto>.Failure(new BadRequestException(ErrorMessage));
				}
				return ServiceResult<TransactionDto>.Failure(
					new ServerErrorException($"Transaction {transactDto.TransactionStatus} could not be created."));
			}
		}
		#endregion

		#region update transaction in the  DB
		public async Task<ServiceResult<TransactionDto>> UpdateTransactionUsingTransactionID(string id, TransactionDto tDto)
		{
			if (tDto == null) return ServiceResult<TransactionDto>.Failure(
				new BadRequestException("configuration data is required"));

			if (tDto.Id != id) return ServiceResult<TransactionDto>.Failure(
					new BadRequestException($"configuration with ID: {id} is not the same as configuration with ID: {tDto.Id}"));

			var transInDb = await _context.tbl_Transactions.FirstOrDefaultAsync(c => c.Id == id);

			if (transInDb == null) return ServiceResult<TransactionDto>.Failure(
				new NotFoundException($"Transaction with ID: {id} not found."));

			if (transInDb.TransactionStatus > 9) return ServiceResult<TransactionDto>.Failure(
			new BadRequestException($"Sale is already closed. Cannot update"));
			try
			{
				// Updating the fields
				transInDb.TransactionDate = tDto.TransactionDate ?? transInDb.TransactionDate;
				transInDb.SoldBy = tDto.SoldBy ?? transInDb.SoldBy;
				transInDb.SaleTotal = tDto.SaleTotal ?? transInDb.SaleTotal;
				transInDb.Change = tDto.Change ?? transInDb.Change;
				transInDb.QuotationId = tDto.QuotationId ?? transInDb.QuotationId;
				transInDb.ShiftId = tDto.ShiftId ?? transInDb.ShiftId;
				transInDb.CustomerId = tDto.CustomerId ?? transInDb.CustomerId;
				transInDb.TransactionStatus = tDto.TransactionStatus ?? transInDb.TransactionStatus;
				transInDb.SaleAgentId = tDto.SaleAgentId ?? transInDb.SaleAgentId;
				transInDb.OrderStatus = tDto.OrderStatus ?? transInDb.OrderStatus;
				transInDb.TransactionComment = tDto.TransactionComment ?? transInDb.TransactionComment;

				_context.tbl_Transactions.Update(transInDb);
				await _context.SaveChangesAsync();

				return ServiceResult<TransactionDto>.Success(transInDb.Adapt<TransactionDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Transaction with status {TransactionStatus} could not be updated.{Error}", transInDb.TransactionStatus, ex);
				return ServiceResult<TransactionDto>.Failure(new ServerErrorException($"configuration could not be created: {ex.Message}"));
			}
		}
		#endregion

		#region update transaction in the  DB
		public async Task<ServiceResult<TransactionDto>> UpdateTransactionOrderStatusAndComment(TransactionStatusUpdateDto tDto)
		{
			if (tDto == null) return ServiceResult<TransactionDto>.Failure(
				new BadRequestException("configuration data is required"));

			var transInDb = await _context.tbl_Transactions.FirstOrDefaultAsync(c => c.Id == tDto.Id);

			if (transInDb == null) return ServiceResult<TransactionDto>.Failure(
				new NotFoundException($"Transaction with ID: {tDto.Id} not found."));

			if (transInDb.TransactionStatus > 9) return ServiceResult<TransactionDto>.Failure(
			new BadRequestException($"Sale is already closed. Cannot update"));

			try
			{
				// Updating the order status
				transInDb.OrderStatus = tDto.OrderStatus ?? transInDb.OrderStatus;
				transInDb.TransactionComment = !string.IsNullOrEmpty(tDto.TransactionComment) ? tDto.TransactionComment : transInDb.TransactionComment;

				await _context.SaveChangesAsync();

				var transactDto = transInDb.Adapt<TransactionDto>();

				return ServiceResult<TransactionDto>.Success(transactDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Transaction with status {TransactionStatus} could not be updated.{Error}", transInDb.TransactionStatus, ex);
				return ServiceResult<TransactionDto>.Failure(new ServerErrorException($"configuration could not be created."));
			}
		}
		#endregion

		#region Add Customer to Transaction
		public async Task<ServiceResult<TransactionDto>> AddCustomerToTransaction(string transactionId, string customerId)
		{
			if (string.IsNullOrEmpty(transactionId) || string.IsNullOrEmpty(customerId))
			{
				return ServiceResult<TransactionDto>.Failure(
					new BadRequestException("Transaction ID and Customer ID are required."));
			}
			try
			{
				var transaction = await _context.tbl_Transactions.FirstOrDefaultAsync(x => x.Id == transactionId);
				if (transaction == null)
				{
					return ServiceResult<TransactionDto>.Failure(
						new NotFoundException($"Transaction with ID: {transactionId} not found."));
				}
				var customer = await _context.tbl_Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId);
				if (customer == null)
				{
					return ServiceResult<TransactionDto>.Failure(
						new NotFoundException($"Customer with ID: {customerId} not found."));
				}
				transaction.CustomerId = customerId;
				await _context.SaveChangesAsync();
				var transactionDto = transaction.Adapt<TransactionDto>();
				return ServiceResult<TransactionDto>.Success(transactionDto);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error adding customer to transaction {transactionId} {error}", transactionId, ex);
				return ServiceResult<TransactionDto>.Failure(new ServerErrorException($"Error adding customer to transaction: {ex.Message}"));
			}
		}
		#endregion

		#region update transaction status and create new transaction
		public async Task<ServiceResult<TransactionDto>> UpdateTransactionStatusAndCreateNewTransaction(TransactionStatusUpdateDto tDto)
		{
			var strategy = _context.Database.CreateExecutionStrategy();
			return await strategy.ExecuteAsync(async () =>
			{
				using (var transaction = await _context.Database.BeginTransactionAsync())
				{
					if (tDto == null) return ServiceResult<TransactionDto>.Failure(
								new BadRequestException("Transaction update data is required"));
					var transInDb = await _context.tbl_Transactions.FirstOrDefaultAsync(c => c.Id == tDto.Id);
					if (transInDb == null) return ServiceResult<TransactionDto>.Failure(
						new NotFoundException($"Transaction with ID: {tDto.Id} not found."));
					if (transInDb.TransactionStatus > 9) return ServiceResult<TransactionDto>.Failure(
						new BadRequestException($"Sale is already closed. Cannot update"));
					try
					{
						transInDb.TransactionStatus = tDto.TransactionStatus ?? transInDb.TransactionStatus;
						transInDb.CustomerId = tDto.CustomerId ?? transInDb.CustomerId;
						transInDb.TransactionComment = !string.IsNullOrEmpty(tDto.TransactionComment) ? tDto.TransactionComment : transInDb.TransactionComment;
						transInDb.Customer = null; // Avoid recursion in mapping
						transInDb.Seller = null;

						_context.tbl_Transactions.Update(transInDb);
						await _context.SaveChangesAsync();

						var shift = await _context.tbl_Shifts.FirstOrDefaultAsync(s => s.Id == tDto.ShiftId);
						if (shift == null)
						{
							_logger.LogError("Transaction with ID {TransactionId} is not linked to any shift.", tDto.Id);
							return ServiceResult<TransactionDto>.Failure(
								new BadRequestException("Transaction not linked to any shift"));
						}

						if (!string.IsNullOrEmpty(shift.SubActiveId))
						{
							var newTransaction = await _context.tbl_Transactions.FirstOrDefaultAsync(t => t.Id == shift.SubActiveId);
							if (newTransaction != null)
							{
								shift.ActiveId = newTransaction.Id;
								newTransaction.TransactionStatus = (int)statics.SaleStatus.opened;
								newTransaction.SoldBy = tDto.SellerId;
								newTransaction.ShiftId = tDto.ShiftId;
								shift.SubActiveId = null;
								await _context.SaveChangesAsync();
								await transaction.CommitAsync();
								return ServiceResult<TransactionDto>.Success(newTransaction.Adapt<TransactionDto>());
							}
						}

						TransactionDto saleDto = new()
						{
							SoldBy = tDto.SellerId,
							ShiftId = tDto.ShiftId,
							TransactionStatus = (int)statics.SaleStatus.opened,
						};
						var saleResult = await CreateNewTransaction(saleDto);
						if (!saleResult.IsSuccess)
						{
							return ServiceResult<TransactionDto>.Failure(saleResult.Error);
						}
						shift.ActiveId = saleResult.Data.Id;
						saleDto = saleResult.Data;

						await _context.SaveChangesAsync();

						await transaction.CommitAsync();

						return ServiceResult<TransactionDto>.Success(saleDto);

					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						_logger.LogError("Transaction update {TransactionUpdate} could not be completed.{Error}", tDto, ex);
						return ServiceResult<TransactionDto>.Failure(
							new ServerErrorException($"Transaction status could not be updated"));
					}
				}
			});
		}
		#endregion

		#region Delete Transaction softdelete
		public async Task<ServiceResult<bool>> RefundTransaction(string id)
		{
			var transInDb = await _context.tbl_Transactions.FindAsync(id);

			if (transInDb == null) return ServiceResult<bool>
					.Failure(new NotFoundException($"Transaction with ID: {id} not found."));

			try
			{
				//change delete property
				transInDb.IsDeleted = true;

				await _context.SaveChangesAsync();

				return ServiceResult<bool>.Success(true);
			}
			catch (Exception ex)
			{
				_logger.LogError("Transaction with ID {id} could not be deleted.{Error}", id, ex);
				return ServiceResult<bool>.Failure(
					new ServerErrorException($"Transaction with ID: {id} could not be deleted."));
			}
		}
		#endregion

		#region Read Transactions from Database
		public async Task<ServiceResult<TransactionDto>> GetTransactionFromDB(string id)
		{
			try
			{
				var transaction = await _context.tbl_Transactions
					.FirstOrDefaultAsync(x => x.Id == id);

				if (transaction == null)
				{
					_logger.LogError($"Transaction with ID: {id} not found.");
					return ServiceResult<TransactionDto>.Failure(
						new NotFoundException($"Transaction with ID: {id} not found."));
				}
				var transactionDto = new TransactionDto();
				transaction.Adapt(transactionDto);
				return ServiceResult<TransactionDto>.Success(transactionDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching transaction with ID {id} in db{error}.", id, ex);

				return ServiceResult<TransactionDto>.Failure(
					new ServerErrorException($"Error fetching transaction: {ex.Message}"));
			}
		}
		#endregion

		#region Read Transactions with transaction details from Database
		public async Task<ServiceResult<TransactionDto>> GetTransactionWithDetailsFromDB(string id)
		{
			try
			{
				var Transaction = await _context.tbl_Transactions
					.AsNoTracking()
					.Include(x => x.TransactionDetails)
					.Include(y => y.Seller)
					.Include(x => x.Customer)
					.Where(x => x.Id == id)
					.SingleOrDefaultAsync();

				if (Transaction == null)
				{
					_logger.LogError($"Transaction with ID: {id} not found.");
					return ServiceResult<TransactionDto>.Failure(
						new NotFoundException($"Transaction with ID: {id} not found."));
				}
				var transactionDto = GetTransactionDto(Transaction);

				return ServiceResult<TransactionDto>.Success(transactionDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching transaction with ID {id} in db{error}.", id, ex);

				return ServiceResult<TransactionDto>.Failure(
					new ServerErrorException($"Error fetching transaction with detail."));
			}
		}
		#endregion

		#region Read Completed Transactions from Database using ID
		public async Task<ServiceResult<TransactionDto>> GetCompletedTransactionFromDBUsingID(string saleId)
		{
			try
			{
				var transaction = await _context.tbl_Transactions.Where(x => x.TransactionStatus >= 10 && x.TransactionStatus <= 20)
																	.FirstOrDefaultAsync(x => x.Id == saleId);
				if (transaction == null)
				{
					return ServiceResult<TransactionDto>.Failure(
					new NotFoundException($"Transaction of {saleId} does not exist"));
				}
				var transactionDto = transaction.Adapt<TransactionDto>();

				return ServiceResult<TransactionDto>.Success(transactionDto);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching completed transactions from db {error}", ex);
				return ServiceResult<TransactionDto>.Failure(
					new ServerErrorException($"Error fetching completed transactions."));
			}
		}
		#endregion

		#region Read Completed Transactions from Database
		public async Task<ServiceResult<PaginationDetails<TransactionDto>>> GetTransactionsFromDB(int offSet, int limit, bool? completed, int? saleStatus, string sortByColumn, bool sortAscending, CancellationToken cancellation)
		{
			try
			{
				var query = _context.tbl_Transactions.AsQueryable();
				if (completed == true)
				{
					query = query.Where(x => x.TransactionStatus >= 10 && x.TransactionStatus <= 20);
				}
				else if (completed == false)
				{
					query = query.Where(x => x.TransactionStatus < 10);
				}
				if (saleStatus.HasValue && saleStatus.Value > 0)
				{
					query = query.Where(x => x.TransactionStatus == saleStatus.Value);
				}
				var transactions = await _context.tbl_Transactions.Where(x => x.TransactionStatus >= 10 && x.TransactionStatus <= 20).Include(x => x.Seller)
															.ToPaginatedResultAsync(offSet, limit, cancellation, sortByColumn, sortAscending);

				return ServiceResult<PaginationDetails<TransactionDto>>.Success(transactions.Adapt<PaginationDetails<TransactionDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching transactions in db {error}", ex);
				return ServiceResult<PaginationDetails<TransactionDto>>.Failure(
					new ServerErrorException($"Error fetching transactions."));
			}
		}
		#endregion

		#region Search Transactions from Database
		public async Task<ServiceResult<PaginationDetails<TransactionDto>>> SearchTransactionsFromDB(string keywords, string userId, string shiftId, string customerId, string orderStatus, bool? Completed, int? saleStatus, DateTime startDate, DateTime endDate, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellation)
		{
			IQueryable<tbl_Transaction> query = _context.tbl_Transactions
				.Where(x => x.TransactionDate == null || // Incomplete transactions have no date
				(startDate <= x.TransactionDate && endDate >= x.TransactionDate));
			try
			{

				if (!string.IsNullOrEmpty(keywords))
				{
					query = query.Where(x => x.Id.ToString() == keywords ||
										x.SaleTotal.ToString() == keywords ||

						(x.Seller != null && x.Seller.FirstName != null && x.Seller.FirstName.Contains(keywords)) ||
						(x.Seller != null && x.Seller.LastName != null && x.Seller.LastName.Contains(keywords)) ||
						(x.Seller != null && x.Seller.UserName != null && x.Seller.UserName.Contains(keywords)) ||
						(x.TransactionComment != null && x.TransactionComment.Contains(keywords)));
				}
				if (!string.IsNullOrEmpty(userId))
				{
					query = query.Where(x => x.SoldBy == userId);
				}
				if (!string.IsNullOrEmpty(shiftId))
				{
					query = query.Where(x => x.ShiftId == shiftId);
				}
				if (!string.IsNullOrEmpty(customerId))
				{
					query = query.Where(x => x.CustomerId == customerId);
				}
				if (!string.IsNullOrEmpty(orderStatus))
				{
					query = query.Where(x => x.OrderStatus == orderStatus);
				}

				if (Completed == true)
				{
					query = query.Where(x => x.TransactionStatus >= 10 && x.TransactionStatus <= 20).Include(x => x.Seller);
				}
				else if (Completed == false)
				{
					query = query.Where(x => x.TransactionStatus < 10).Include(x => x.Seller);
				}
				if (saleStatus.HasValue && saleStatus.Value > 0)
				{
					query = query.Where(x => x.TransactionStatus == saleStatus.Value);
				}
				var transactions = await query.ToPaginatedResultAsync(offSet, limit, cancellation, sortByColumn, sortAscending);

				return ServiceResult<PaginationDetails<TransactionDto>>.Success(transactions.Adapt<PaginationDetails<TransactionDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error searching transactions in db {error}", ex);
				return ServiceResult<PaginationDetails<TransactionDto>>.Failure(
					new ServerErrorException($"Error searching transactions."));
			}
		}
		#endregion

		#region Read Completed Transactions from Database using date range
		public async Task<ServiceResult<PaginationDetails<TransactionDto>>> GetCompletedTransactionsFromDBUsingDateRange(DateTime startDate, DateTime endDate, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
		{
			try
			{
				var transactions = await _context.tbl_Transactions.Where(x => x.TransactionDate >= startDate && x.TransactionDate >= endDate
																		&& x.TransactionStatus >= 10 && x.TransactionStatus <= 20)
																  .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);


				return ServiceResult<PaginationDetails<TransactionDto>>.Success(transactions.Adapt<PaginationDetails<TransactionDto>>());
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching transactions by date range in db {error}", ex);
				return ServiceResult<PaginationDetails<TransactionDto>>.Failure(
					new ServerErrorException($"Error fetching transactions by date range."));
			}
		}
		#endregion

		#region Read PENDING Transactions from Database 
		public async Task<ServiceResult<List<TransPendingDto>>> GetPendingTransactionFromDBUsingUserID(string UserID)
		{
			try
			{
				string sql = @"
					SELECT DISTINCT tbl_transaction.*, tbl_Customers.FullName 
					FROM tbl_transaction 
					LEFT JOIN tbl_Customers ON tbl_transaction.customerID = tbl_Customers.CustomerID 
					LEFT JOIN tbl_transactionDetail ON tbl_transaction.transactionId = tbl_transactionDetail.transactionID 
					INNER JOIN tbl_products ON tbl_transactionDetail.productID = tbl_products.productId 
					WHERE tbl_transaction.transactionStatus = @transactionStatus 
				OR tbl_transaction.transactionStatus = @transactionStatus1 AND tbl_transaction.soldBy = @soldBy";

				var parameters = new List<SqlParameter>
				{
					new SqlParameter("@transactionStatus", (int)statics.SaleStatus.OnHold),
					new SqlParameter("@transactionStatus1", (int)statics.SaleStatus.Quotation),
					new SqlParameter("@soldBy", UserID)
				};

				//if (UserID != 123)
				//{
				//	sql += "AND tbl_transaction.soldBy = @soldBy";
				//	parameters.Add(new SqlParameter("@soldBy", UserID));
				//}

				var transPending = await _context.Database.SqlQueryRaw<TransPendingDto>(sql, parameters.ToArray()).ToListAsync();

				return ServiceResult<List<TransPendingDto>>.Success(transPending);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching pending transactions in db {error}", ex);
				return ServiceResult<List<TransPendingDto>>.Failure(
					new ServerErrorException($"Error fetching pending transactions."));
			}
		}
		#endregion

		#region search PENDING Transactions from Database based On USerID
		public async Task<ServiceResult<List<TransPendingDto>>> SearchPendingTransactions(string keywords, string UserID, int transactionStatus, int OrderStatus)
		{
			try
			{
				string sql;
				if (transactionStatus != 123)
				{
					sql = "SELECT DISTINCT tbl_transaction.*, tbl_Customers.FullName FROM tbl_transaction left join tbl_Customers on tbl_transaction.customerID = tbl_Customers.CustomerID left join tbl_transactionDetail on tbl_transaction.transactionId = tbl_transactionDetail.transactionID inner join tbl_products on tbl_transactionDetail.productID = tbl_products.productId WHERE transactionStatus=@transactionStatus  ";
				}
				else
				{
					sql = "SELECT DISTINCT tbl_transaction.*, tbl_Customers.FullName FROM tbl_transaction left join tbl_Customers on tbl_transaction.customerID = tbl_Customers.CustomerID left join tbl_transactionDetail on tbl_transaction.transactionId = tbl_transactionDetail.transactionID inner join tbl_products on tbl_transactionDetail.productID = tbl_products.productId WHERE (tbl_transaction.transactionStatus=@transactionStatus1 OR  tbl_transaction.transactionStatus=@transactionStatus2)  ";
				}

				if (!string.IsNullOrEmpty(UserID))
				{
					sql = sql + " and tbl_transaction.soldBy=@soldBy";

				}
				if (OrderStatus != 123)
				{
					sql = sql + " and tbl_transaction.OrderStatus=@OrderStatus";

				}
				if (!string.IsNullOrEmpty(keywords))
				{
					sql = sql + " AND (tbl_transaction.transactionId LIKE @keywords OR tbl_Customers.FullName LIKE @keywords OR tbl_products.productName LIKE @keywords ) ";

				}


				sql = sql + " order by transactionDate desc";

				var parameters = new List<SqlParameter>
				{
					new SqlParameter("@keywords", "%" + keywords + "%"),
					new SqlParameter("@transactionStatus", transactionStatus),
					new SqlParameter("@transactionStatus1", (int)statics.SaleStatus.Quotation),
					new SqlParameter("@transactionStatus2", (int)statics.SaleStatus.OnHold),
					new SqlParameter("@soldBy", UserID),
					new SqlParameter("@soldBy", OrderStatus)
				};

				if (UserID != "123")
				{
					parameters.Add(new SqlParameter("@UserID", UserID));

				}

				var transPending = await _context.Database.SqlQueryRaw<TransPendingDto>(sql, parameters.ToArray()).ToListAsync();


				return ServiceResult<List<TransPendingDto>>.Success(transPending);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error searching pending transactions in db {error}", ex);
				return ServiceResult<List<TransPendingDto>>.Failure(
					new ServerErrorException($"Error searching pending transactions."));
			}

		}
		#endregion

		#region SELECT MOST RECENT Transactions from Database based On TRANSACTION ID
		public async Task<ServiceResult<List<TransPendingDto>>> GetLastTransactionIDFromDB(string userID)
		{
			int saleStatus = (int)statics.SaleStatus.ClosedInstantly;
			int saleStatus1 = (int)statics.SaleStatus.Quotation;
			int saleStatus2 = (int)statics.SaleStatus.ClosedFromHold;
			try
			{
				string sql = "SELECT top 5 * FROM tbl_transaction WHERE transactionStatus= @saleStatus  OR transactionStatus= @saleStatus1 OR transactionStatus= @saleStatus2 and soldBy=@soldBy ";

				sql = sql + " order by transactionDate desc";

				var parameters = new List<SqlParameter>
				{
					new SqlParameter("@saleStatus", saleStatus),
					new SqlParameter("@saleStatus2", saleStatus2),
					new SqlParameter("@saleStatus1", saleStatus1),
					new SqlParameter("@soldBy",userID)
				};

				var transPending = await _context.Database.SqlQueryRaw<TransPendingDto>(sql, parameters.ToArray()).ToListAsync();

				return ServiceResult<List<TransPendingDto>>.Success(transPending);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching last transaction from by user Id {id} db {error}", userID, ex);
				return ServiceResult<List<TransPendingDto>>.Failure(
					new ServerErrorException($"Error fetching last transaction from by user Id: {userID}"));
			}
		}
		#endregion

		#region Get Customer Debits SUM from Database based on customerID and TransactionDate
		public async Task<ServiceResult<decimal>> GetCustomerDebitsLowerThanEndDate(TransactionDto t, DateTime EndDate)
		{
			try
			{
				string sql = "select COALESCE(SUM(saleTotal), 0) from tbl_transaction where transactionStatus=@transactionStatus and customerID=@CustomerID and transactionDate <= @transactionDate";

				var parameters = new[]
				{
					new SqlParameter("@transactionStatus", t.TransactionStatus),
					new SqlParameter("@CustomerID", t.CustomerId),
					new SqlParameter("@transactionDate", EndDate)
				};

				decimal debts = await _context.Database.SqlQueryRaw<decimal>(sql, parameters).FirstOrDefaultAsync();

				return ServiceResult<decimal>.Success(debts);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching customer debts in db {error}", ex);
				return ServiceResult<decimal>.Failure(
					new ServerErrorException($"Error fetching customer debts."));
			}
		}
		#endregion

		#region Get Sum Of revenue for  from Database Using Date Range 
		public async Task<ServiceResult<decimal>> GetTotalRevenueFromDBUsingDateRange(DateTime startDate, DateTime endDate)
		{
			try
			{
				string sql = "select COALESCE(Sum(saleTotal),0) from tbl_transaction where transactionDate >= @startDate and transactionDate <= @endDate and transactionStatus >= 10 AND  transactionStatus <=20";

				var parameters = new[]
				{
					new SqlParameter("@startDate", startDate),
					new SqlParameter("@endDate", endDate)
				};

				decimal sum = await _context.Database.SqlQueryRaw<decimal>(sql, parameters).FirstOrDefaultAsync();

				return ServiceResult<decimal>.Success(sum);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching transaction revenue in db {error}", ex);
				return ServiceResult<decimal>.Failure(
					new ServerErrorException($"Error fetching transaction revenue: {ex.Message}"));
			}

		}
		#endregion

		#region Get Sum Of No. of Sales for  from Database Using Date Range 
		public async Task<ServiceResult<int>> GetTotalSalesNoFromDBUsingDateRange(DateTime startDate, DateTime endDate)
		{
			try
			{
				string sql = "select COALESCE(Count(transactionID),0) from tbl_transaction where transactionDate >= @startDate and transactionDate <= @endDate and transactionStatus >= 10 AND  transactionStatus <=20";

				var parameters = new[]
				{
					new SqlParameter("@startDate",startDate),
					new SqlParameter("@endDate", endDate)
				};

				int count = await _context.Database.SqlQueryRaw<int>(sql, parameters).FirstOrDefaultAsync();

				return ServiceResult<int>.Success(count);

			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching transaction count in db {error}", ex);
				return ServiceResult<int>.Failure(
					new ServerErrorException($"Error fetching transaction count."));
			}

		}
		#endregion

		#region Get Sum Of revenue for Today from Database using date range
		public async Task<ServiceResult<decimal>> GetSumOfRevenueFromDForToday()
		{
			DateTime todaysDate = DateTime.Today;
			try
			{
				string sql = "select COALESCE(Sum(saleTotal),0) from tbl_transaction where transactionDate >= @startDate and transactionDate <= @endDate and transactionStatus >= 10 AND  transactionStatus <=20";

				var parameters = new[]
				{
					new SqlParameter("@startDate",todaysDate),
					new SqlParameter("@endDate", todaysDate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999))
				};

				decimal revenue = await _context.Database.SqlQueryRaw<decimal>(sql, parameters).FirstOrDefaultAsync();

				return ServiceResult<decimal>.Success(revenue);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching today's revenue in db {error}", ex);
				return ServiceResult<decimal>.Failure(
					new ServerErrorException($"Error fetching today's revenue."));
			}
		}
		#endregion

		#region manual mapping of Transaction Dto
		public TransactionDto GetTransactionDto(tbl_Transaction transaction)
		{
			// Initialize the DTO
			var transactionDto = new TransactionDto
			{
				Id = transaction.Id,
				TransactionDate = transaction.TransactionDate,
				SoldBy = transaction.SoldBy,
				SaleTotal = transaction.SaleTotal,
				Change = transaction.Change,
				ShiftId = transaction.ShiftId,
				CustomerId = transaction.CustomerId,
				TransactionStatus = transaction.TransactionStatus,
				SaleAgentId = transaction.SaleAgentId,
				QuotationId = transaction.QuotationId,
				OrderStatus = transaction.OrderStatus,
				TransactionComment = transaction.TransactionComment,
				Seller = transaction.Seller != null ? new AppUserDto
				{
					FirstName = transaction.Seller.FirstName,
					LastName = transaction.Seller.LastName,
					Address = transaction.Seller.Address,
					Aboutme = transaction.Seller.Aboutme,
					Contacts = transaction.Seller.Contacts,
					ProfilePicUrl = transaction.Seller.ProfilePicUrl,
					CoverPhotoUrl = transaction.Seller.CoverPhotoUrl,

				} : null,
				TransactionDetails = transaction.TransactionDetails?.Select(detail => new TransactionDetailDto
				{
					Id = detail.Id,
					ProductId = detail.ProductId,
					Qty = detail.Qty,
					CostExc = detail.CostExc,
					CostInc = detail.CostInc,
					PriceInc = detail.PriceInc,
					PriceExc = detail.PriceExc,
					TaxId = detail.TaxId,
					TaxPercent = detail.TaxPercent,
					DiscountId = detail.DiscountId,
					DiscountPercent = detail.DiscountPercent,
					TransactionId = detail.TransactionId,
					TotalPriceInc = detail.TotalPriceInc,
					TotalPriceExc = detail.TotalPriceExc,
					SortOrder = detail.SortOrder,
					CostIncState = detail.CostIncState,
					SpecialPricingUsed = detail.SpecialPricingUsed,
					// No recursion: Avoid mapping back the Transaction property
				}).ToList()
			};

			// Manually map Customer (avoid recursion with Transactions)
			if (transaction.Customer != null)
			{
				transactionDto.Customer = new CustomerDto
				{
					Id = transaction.Customer.Id,
					AccountNumber = transaction.Customer.AccountNumber,
					FullName = transaction.Customer.FullName,
					Contact = transaction.Customer.Contact,
					CardNumber = transaction.Customer.CardNumber,
					VatNumber = transaction.Customer.VatNumber,
					Email = transaction.Customer.Email,
					Address = transaction.Customer.Address,
					CreditLimit = transaction.Customer.CreditLimit,
					Company = transaction.Customer.Company
					// No recursion: Exclude the Transactions property to prevent an infinite loop
				};
			}

			return transactionDto;
		}
		#endregion

		#region Get Sales ForCSVExport BasedOn SelectedFields

		public async Task<ServiceResult<MemoryStream>> GetTransactionsForCSVExportBySelectedFields(List<string> selectedColumnNames)
		{
			try
			{
				IQueryable<tbl_Transaction> query = _context.tbl_Transactions;

				// Build the dynamic SELECT clause
				var selectFields = new List<string>();
				var properties = typeof(tbl_Transaction).GetProperties();
				foreach (var prop in properties)
				{
					if (selectedColumnNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
						selectFields.Add(prop.Name);
				}

				// Project only the selected columns dynamically
				var dynamicQuery = query.Select($"new ({string.Join(", ", selectFields)})");

				var exportObject = dynamicQuery.Adapt<List<TransactionsExportDto>>();
				//create excel file and return it
				var memorystream = await _excelDomainService.ExportExcelRecords(exportObject, selectedColumnNames, "Transactions");

				await Task.CompletedTask;

				return ServiceResult<MemoryStream>.Success(memorystream);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while fetching transactions for csv export: {ex}", ex);
				return ServiceResult<MemoryStream>.Failure(
					new ServerErrorException("Could not fetch transactions for csv export"));
			}
		}
		#endregion

		#region Import Transactions from Excel
		public async Task<ServiceResult<ImportResultSummary>> ImportTransactionsFromExcel(ImportDataDto p)
		{
			if (p == null)
				return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("Import data is required."));

			int totalTransactions = 0;
			int updatedCount = 0;
			int createdCount = 0;
			int failedCount = 0;
			List<string> messages = new List<string>();

			if (p.UploadedExcelContent == null || p.UploadedExcelContent.Count == 0)
			{
				return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No data found in the uploaded file."));
			}

			foreach (var catInList in p.UploadedExcelContent)
			{
				totalTransactions++;
				try
				{
					tbl_Transaction? transactionEntity = null;
					bool isUpdate = false;

					if (p.ColumnMappingsList == null || p.ColumnMappingsList.Count == 0)
					{
						return ServiceResult<ImportResultSummary>.Failure(new BadRequestException("No column mappings provided."));
					}
					var transactionIdKey = GetKey("TransactionId", p.ColumnMappingsList);
					var transactionDateKey = GetKey("TransactionId", p.ColumnMappingsList);
					string transactionIdStr = !string.IsNullOrEmpty(transactionIdKey) ? GetValue(catInList, transactionIdKey)?.ToString()! : string.Empty;
					string transactionDateStr = !string.IsNullOrEmpty(transactionDateKey) ? GetValue(catInList, transactionDateKey)?.ToString()! : string.Empty;
					if (string.IsNullOrEmpty(transactionIdStr) && string.IsNullOrEmpty(transactionDateStr))
					{
						string description = BuildTransactionDescription(catInList, p);
						messages.Add($"&#x1F4CC; {description} could not be processed due to missing Transaction name.");
						failedCount++;
						continue;
					}
					if (!string.IsNullOrEmpty(transactionIdStr))
					{
						transactionEntity = await _context.tbl_Transactions.FirstOrDefaultAsync(c => c.Id == transactionIdStr);
					}
					if (transactionEntity != null)
					{
						isUpdate = true;
					}
					else
					{
						// Check if name is unique (for creation)
						bool exists = await _context.tbl_Transactions.AnyAsync(c => c.TransactionDate.ToString() == transactionDateStr);
						if (exists)
						{
							string description = BuildTransactionDescription(catInList, p);
							messages.Add($"&#x1F4CC; {description} could not be added as the transaction date '{transactionDateStr}' already exists.");
							failedCount++;
							continue;
						}
						// Create new transaction
						transactionEntity = new tbl_Transaction();
						_context.tbl_Transactions.Add(transactionEntity);
						isUpdate = false;
					}

					// Map fields from Excel data to transaction entity
					foreach (var mapping in p.ColumnMappingsList)
					{
						string systemColumn = mapping.SystemColumn.ToLower();
						string fileColumn = mapping.SelectedFileColumn;
						if (string.IsNullOrEmpty(fileColumn))
							continue;

						object value = GetValue(catInList, fileColumn);

						switch (systemColumn)
						{
							case "transactiondate":
								if (value != null && DateTime.TryParse(value.ToString(), out DateTime transactionDate))
									transactionEntity.TransactionDate = transactionDate;
								break;
							case "soldby":
								transactionEntity.SoldBy = value?.ToString();
								break;
							case "saletotal":
								if (value != null && decimal.TryParse(value.ToString(), out decimal saleTotal))
									transactionEntity.SaleTotal = saleTotal;
								break;
							case "change":
								if (value != null && decimal.TryParse(value.ToString(), out decimal change))
									transactionEntity.Change = change;
								break;
							case "shiftid":
								if (value != null && !string.IsNullOrEmpty(value.ToString()))
									transactionEntity.ShiftId = value.ToString();
								break;
							case "customerid":
								if (value != null && !string.IsNullOrEmpty(value.ToString()))
									transactionEntity.CustomerId = value.ToString();
								break;
							case "transactionstatus":
								if (value != null && int.TryParse(value.ToString(), out int transactionStatus))
									transactionEntity.TransactionStatus = transactionStatus;
								break;
							case "saleagentid":
								transactionEntity.SaleAgentId = value?.ToString();
								break;
							case "quotationid":
								if (value != null && !string.IsNullOrEmpty(value.ToString()))
									transactionEntity.QuotationId = value.ToString();
								break;
							//case "orderstatus":
							//    if (value != null && int.TryParse(value.ToString(), out int orderStatus))
							//        transactionEntity.OrderStatus = orderStatus;
							//    break;
							case "transactioncomment":
								transactionEntity.TransactionComment = value?.ToString();
								break;
								// Add more cases as needed for additional properties
						}
					}

					await _context.SaveChangesAsync();

					if (isUpdate)
						updatedCount++;
					else
						createdCount++;
				}
				catch (Exception ex)
				{
					string description = BuildTransactionDescription(catInList, p);
					messages.Add($"&#x1F4CC; {description} could not be imported due to {ex.Message}");
					failedCount++;
				}
			}

			string summary = $"Total Transactions Processed: {totalTransactions}\n\nCreated: {createdCount}\nUpdated: {updatedCount}\nFailed: {failedCount}";
			string resultMessage = string.Join("\n", messages);

			var output = new ImportResultSummary
			{
				Summary = summary,
				Errors = resultMessage
			};

			return ServiceResult<ImportResultSummary>.Success(output);
		}
		private string BuildTransactionDescription(Dictionary<string, object> catData, ImportDataDto p)
		{
			List<string> parts = new List<string>();

			var transactionIdKey = GetKey("TransactionId", p.ColumnMappingsList);
			var transactionDateKey = GetKey("TransactionDate", p.ColumnMappingsList);
			var soldByKey = GetKey("SoldBy", p.ColumnMappingsList);
			var saleTotalKey = GetKey("SaleTotal", p.ColumnMappingsList);
			var changeKey = GetKey("Change", p.ColumnMappingsList);
			var shiftIdKey = GetKey("ShiftId", p.ColumnMappingsList);
			var customerIdKey = GetKey("CustomerId", p.ColumnMappingsList);
			var transactionStatusKey = GetKey("TransactionStatus", p.ColumnMappingsList);
			var saleAgentIdKey = GetKey("SaleAgentId", p.ColumnMappingsList);
			var quotationIdKey = GetKey("QuotationId", p.ColumnMappingsList);
			var orderStatusKey = GetKey("OrderStatus", p.ColumnMappingsList);
			var transactionCommentKey = GetKey("TransactionComment", p.ColumnMappingsList);

			var transactionIdVal = !string.IsNullOrEmpty(transactionIdKey) ? GetValue(catData, transactionIdKey)?.ToString() : "";
			var transactionDateVal = !string.IsNullOrEmpty(transactionDateKey) ? GetValue(catData, transactionDateKey)?.ToString() : "";
			var soldByVal = !string.IsNullOrEmpty(soldByKey) ? GetValue(catData, soldByKey)?.ToString() : "";
			var saleTotalVal = !string.IsNullOrEmpty(saleTotalKey) ? GetValue(catData, saleTotalKey)?.ToString() : "";
			var changeVal = !string.IsNullOrEmpty(changeKey) ? GetValue(catData, changeKey)?.ToString() : "";
			var shiftIdVal = !string.IsNullOrEmpty(shiftIdKey) ? GetValue(catData, shiftIdKey)?.ToString() : "";
			var customerIdVal = !string.IsNullOrEmpty(customerIdKey) ? GetValue(catData, customerIdKey)?.ToString() : "";
			var transactionStatusVal = !string.IsNullOrEmpty(transactionStatusKey) ? GetValue(catData, transactionStatusKey)?.ToString() : "";
			var saleAgentIdVal = !string.IsNullOrEmpty(saleAgentIdKey) ? GetValue(catData, saleAgentIdKey)?.ToString() : "";
			var quotationIdVal = !string.IsNullOrEmpty(quotationIdKey) ? GetValue(catData, quotationIdKey)?.ToString() : "";
			var orderStatusVal = !string.IsNullOrEmpty(orderStatusKey) ? GetValue(catData, orderStatusKey)?.ToString() : "";
			var transactionCommentVal = !string.IsNullOrEmpty(transactionCommentKey) ? GetValue(catData, transactionCommentKey)?.ToString() : "";

			if (!string.IsNullOrEmpty(transactionIdVal))
				parts.Add($"ID: {transactionIdVal}");
			if (!string.IsNullOrEmpty(transactionDateVal))
				parts.Add($"Date: {transactionDateVal}");
			if (!string.IsNullOrEmpty(soldByVal))
				parts.Add($"SoldBy: {soldByVal}");
			if (!string.IsNullOrEmpty(saleTotalVal))
				parts.Add($"SaleTotal: {saleTotalVal}");
			if (!string.IsNullOrEmpty(changeVal))
				parts.Add($"Change: {changeVal}");
			if (!string.IsNullOrEmpty(shiftIdVal))
				parts.Add($"ShiftId: {shiftIdVal}");
			if (!string.IsNullOrEmpty(customerIdVal))
				parts.Add($"CustomerId: {customerIdVal}");
			if (!string.IsNullOrEmpty(transactionStatusVal))
				parts.Add($"Status: {transactionStatusVal}");
			if (!string.IsNullOrEmpty(saleAgentIdVal))
				parts.Add($"SaleAgentId: {saleAgentIdVal}");
			if (!string.IsNullOrEmpty(quotationIdVal))
				parts.Add($"QuotationId: {quotationIdVal}");
			if (!string.IsNullOrEmpty(orderStatusVal))
				parts.Add($"OrderStatus: {orderStatusVal}");
			if (!string.IsNullOrEmpty(transactionCommentVal))
				parts.Add($"Comment: {transactionCommentVal}");

			return "transaction [" + string.Join(", ", parts) + "]";
		}
		private object GetValue(Dictionary<string, object> item, string key)
		{
			return item.TryGetValue(key, out object? value) ? (value == null ? "" : value) : "";
		}
		private string GetKey(string columnName, List<ColumnMapping> mappings)
		{
			return mappings.FirstOrDefault(x => x.SystemColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))!.SelectedFileColumn;
		}
		public static string NormalizeString(string? input)
		{
			return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim().ToLowerInvariant();
		}

		public static bool CompareNormalizedStrings(string? str1, string? str2)
		{
			return NormalizeString(str1) == NormalizeString(str2);
		}

		#endregion

		#region Check if transaction is empty (has no items)
		public async Task<ServiceResult<bool>> IsTransactionEmpty(string transactionId)
		{
			try
			{
				if (string.IsNullOrEmpty(transactionId))
					return ServiceResult<bool>.Failure(new BadRequestException("Transaction ID is required"));

				var hasItems = await _context.tbl_TransactionDetails
					.AnyAsync(x => x.TransactionId == transactionId);

				return ServiceResult<bool>.Success(!hasItems);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error checking if transaction {TransactionId} is empty: {Error}", transactionId, ex);
				return ServiceResult<bool>.Failure(new ServerErrorException("Could not check transaction status"));
			}
		}
		#endregion

	}
}
