using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ReportingDto;
using assetlen.Shared.Models.statics;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using assetlen.Shared.Models.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using NPOI.XSSF.Streaming.Values;

namespace assetlen.Service.DbServices
{
    public class ShiftsDAL : IShiftsDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ITransactionDAL _transaction;
        private readonly ILogger<ShiftsDAL> _logger;
        private readonly ITenantProvider _tenantProvider;
        public ShiftsDAL(ILogger<ShiftsDAL> logger, AssetlenDbContext context, ITransactionDAL transaction, ITenantProvider tenantProvider)
        {
            _logger = logger;
            _context = context;
            _transaction = transaction;
            _tenantProvider = tenantProvider;
        }

        private string TenantId => _tenantProvider.GetTenantId();

        #region Read Shifts from Database
        public async Task<ServiceResult<PaginationDetails<ShiftsDto>>> GetShiftsFromDB(DateTime startDate, DateTime endDate, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {

                var result = await _context.tbl_Shifts.Include(x => x.User).AsNoTracking()
                    .Where(x => x.DateTimeOpened >= startDate && x.DateTimeOpened <= endDate)
                    .OrderByDescending(x => x.Id).ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ShiftsDto>>.Success(result.Adapt<PaginationDetails<ShiftsDto>>());
            }
            catch (Exception ex)
            {

                _logger.LogError("Error while getting Shift data : {ex}", ex);
                return ServiceResult<PaginationDetails<ShiftsDto>>.Failure(
                    new ServerErrorException("Could not fetch shifts."));
            }
        }
        #endregion

        #region Read Shifts Cashup Amount per PaymentMode from Database
        public async Task<ServiceResult<List<PaymentModeSummaryDto>>> GetShiftAmountCollectedPerPaymentModeUsingShiftID(string shiftId)
        {

            List<PaymentModeSummaryDto> paymentModes = new List<PaymentModeSummaryDto>();
            List<ChangeSummaryDto> changeSummary = new List<ChangeSummaryDto>();

            try
            {
                // SQL query for main distributions
                string sql = @"WITH UnionTable AS 
                    (
                        SELECT pm.Description, 
                               t.shiftId AS ShiftId, 
                               pm.PaymentModeID as PaymentModeID, 
                               SUM(p.Amount) AS SaleTotal
                        FROM tbl_transaction t
                        INNER JOIN tbl_Payments p ON p.saleID = t.Id
                        INNER JOIN tbl_paymentMode pm ON pm.PaymentModeID = p.PaymentModeID
                        WHERE ((t.transactionStatus = 10) OR 
                               (t.transactionStatus = 11) OR 
                               (t.transactionStatus = 13)) 
                              AND t.shiftId = @shiftID
                        GROUP BY pm.Description, t.shiftId, pm.PaymentModeID
                        UNION ALL
                        SELECT pm.Description, 
                               cd.drawerID AS ShiftId, 
                               pm.PaymentModeID PaymentModeID, 
                               SUM(p.Amount) AS SaleTotal
                        FROM tbl_Payments p
                        INNER JOIN tbl_paymentMode pm ON pm.PaymentModeID = p.PaymentModeID
                        INNER JOIN tbl_customerDeposit cd ON p.CustomerDepositID = cd.Id
                        WHERE cd.drawerID = @shiftID
                        GROUP BY pm.Description, cd.drawerID, pm.PaymentModeID
                    )
                    SELECT Description, ShiftId, PaymentModeID, 
                           SUM(SaleTotal) AS SaleTotal
                    FROM UnionTable
                    GROUP BY Description, ShiftId, PaymentModeID";

                // SQL query for total change given
                string sql2 = @"SELECT pm.Description, 
                               t.shiftId AS ShiftId, 
                               pm.PaymentModeID as PaymentModeID, 
                               SUM(t.change) AS Change
                        FROM tbl_transaction t
                        INNER JOIN tbl_Payments p ON p.saleID = t.Id
                        INNER JOIN tbl_paymentMode pm ON pm.PaymentModeID = p.PaymentModeID
                        WHERE ((t.transactionStatus = 10 AND pm.PaymentModeID = 1) OR 
                               (t.transactionStatus = 11) OR 
                               (t.transactionStatus = 13)) 
                              AND t.shiftId = @shiftID
                        GROUP BY pm.Description, t.shiftId, pm.PaymentModeID";

                // Execute the SQL queries using DbContext.Database
                paymentModes = await _context.Database.SqlQueryRaw<PaymentModeSummaryDto>(sql, new SqlParameter("@shiftID", shiftId)).ToListAsync();
                changeSummary = await _context.Database.SqlQueryRaw<ChangeSummaryDto>(sql2, new SqlParameter("@shiftID", shiftId)).ToListAsync();

                if (changeSummary.Any() && paymentModes.Any())
                {
                    var change = changeSummary.FirstOrDefault()?.Change ?? 0; // Takes FIRST row
                    //var cashChange = changeSummary
                    //.FirstOrDefault(x => x.PaymentModeID == "1")?.Change ?? 0;

                    foreach (var paymentMode in paymentModes)
                    {
                        if (paymentMode.PaymentModeId == "1") // Correct cash entries
                        {
                            paymentMode.SaleTotal -= change;
                            break;
                        }
                    }
                }
                return ServiceResult<List<PaymentModeSummaryDto>>.Success(paymentModes);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting Shift data by shift Id {id} : {ex}", shiftId, ex);
                return ServiceResult<List<PaymentModeSummaryDto>>.Failure(
                    new ServerErrorException($"Could not fetch ammount collected from shift of Id: {shiftId}"));
            }
        }
        #endregion

        #region Get Real-time Shift Performance Report
        public async Task<ServiceResult<List<ShiftPerformanceDto>>> GetShiftPerformanceReport(DateTime reportDate, string? userId = null)
        {
            try
            {
                DateTime startDate = reportDate.Date;
                DateTime endDate = reportDate.Date.AddDays(1).AddTicks(-1);

                if (!string.IsNullOrEmpty(userId))
                {
                    bool userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                    if (!userExists)
                    {
                        return ServiceResult<List<ShiftPerformanceDto>>.Failure(
                            new NotFoundException($"User with ID {userId} does not exist."));
                    }
                }

                string sql = @"
                    DECLARE @TenantId UNIQUEIDENTIFIER = @tenantIdParam;
                    
                    WITH ShiftSales AS (
                        -- Completed sales
                        SELECT
                            s.Id AS ShiftId,
                            u.userName AS Cashier,
                            s.DateTimeOpened,
                            SUM(p.Amount) AS SaleAmount,
                            SUM(CASE WHEN pm.PaymentModeID = 1 THEN t.change ELSE 0 END) AS ChangeGiven
                        FROM tbl_Shifts s
                        INNER JOIN Users u ON u.Id = s.UserId
                        INNER JOIN tbl_transaction t ON t.shiftId = s.Id
                        INNER JOIN tbl_Payments p ON p.saleID = t.Id
                        INNER JOIN tbl_paymentMode pm ON pm.PaymentModeID = p.PaymentModeID
                        WHERE s.DateTimeOpened < @endDate 
                            AND (s.DateTimeClosed > @startDate OR s.DateTimeClosed IS NULL)
                            AND s.TenantId = @TenantId
                            AND COALESCE(s.IsDeleted, 0) = 0
                            AND t.TenantId = @TenantId
                            AND COALESCE(t.IsDeleted, 0) = 0
                            AND p.TenantId = @TenantId
                            AND COALESCE(p.IsDeleted, 0) = 0
                            AND t.transactionStatus IN (10, 11, 13) -- Completed statuses
                            AND (@userId IS NULL OR u.Id = @userId) -- User filter
                            AND s.drawerStatus = 1                  -- only open shifts
                        GROUP BY s.Id, u.userName, s.DateTimeOpened
                        
                        UNION ALL
                        
                        -- Customer deposits
                        SELECT
                            s.Id AS ShiftId,
                            u.userName AS Cashier,
                            s.DateTimeOpened,
                            SUM(p.Amount) AS SaleAmount,
                            0 AS ChangeGiven
                        FROM tbl_Shifts s
                        INNER JOIN Users u ON u.Id = s.UserId
                        INNER JOIN tbl_customerDeposit cd ON cd.drawerID = s.Id
                        INNER JOIN tbl_Payments p ON p.CustomerDepositID = cd.Id
                        WHERE s.TenantId = @TenantId
                            AND COALESCE(s.IsDeleted, 0) = 0
                            AND cd.TenantId = @TenantId
                            AND COALESCE(cd.IsDeleted, 0) = 0
                            AND p.TenantId = @TenantId
                            AND COALESCE(p.IsDeleted, 0) = 0
                            AND (@userId IS NULL OR u.Id = @userId) -- User filter
                        GROUP BY s.Id, u.userName, s.DateTimeOpened
                    )
                    SELECT
                        Cashier,
                        SUM(SaleAmount) - SUM(ChangeGiven) AS Amount,
                        MIN(DateTimeOpened) AS ShiftStartDate
                    FROM ShiftSales
                    GROUP BY ShiftId, Cashier";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@startDate", startDate),
                    new SqlParameter("@endDate", endDate),
                    new SqlParameter("@tenantIdParam", TenantId)
                };

                // Optional userId parameter
                var userIdParam = new SqlParameter("@userId", SqlDbType.NVarChar);
                if (!string.IsNullOrEmpty(userId))
                {
                    userIdParam.Value = userId;
                }
                else
                {
                    userIdParam.Value = DBNull.Value;
                }
                parameters.Add(userIdParam);

                var result = await _context.Database.SqlQueryRaw<ShiftPerformanceDto>(sql, parameters.ToArray()).ToListAsync();
                return ServiceResult<List<ShiftPerformanceDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting shift performance report: {ex}", ex);
                return ServiceResult<List<ShiftPerformanceDto>>.Failure(
                    new ServerErrorException("Could not generate real-time shift report."));
            }
        }
        #endregion

        #region Read ShiftsTotal Amount for all Shiftsfrom Database
        public async Task<ServiceResult<List<ShiftAmountCollectedDto>>> GetShiftAmountCollectedPerShift()
        {


            try
            {
                // SQL query for main distributions
                string sql = @"Select
                              tbl_shifts.userId,
                              tbl_users.userName,
                              Sum(tbl_Payments.Amount) As ShiftTotal,
                              tbl_shifts.dateTimeOpened As ShiftOpened
                            From
                              tbl_users Inner Join
                              tbl_transaction On tbl_transaction.soldBy = tbl_users.userId Inner Join
                              tbl_Payments On tbl_Payments.saleID = tbl_transaction.transactionId Inner Join
                              tbl_shifts On tbl_shifts.shiftId = tbl_transaction.shiftId
                            Where
                              tbl_shifts.drawerStatus = 1
                            Group By
                              tbl_shifts.userId, tbl_users.userName, tbl_shifts.drawerStatus,
                              tbl_shifts.dateTimeOpened ";



                // Execute the SQL queries using DbContext.Database
                var result = await _context.Database.SqlQueryRaw<ShiftAmountCollectedDto>(sql).ToListAsync();


                return ServiceResult<List<ShiftAmountCollectedDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting Shift data : {ex}", ex);
                return ServiceResult<List<ShiftAmountCollectedDto>>.Failure(new ServerErrorException(ex.Message));
            }



        }
        #endregion

        #region Create User shift
        public async Task<ServiceResult<ShiftsDto>> CreateNewShift(ShiftsDto s)
        {
            try
            {

                if (s == null)
                    return ServiceResult<ShiftsDto>.Failure(
                        new ServerErrorException("Shifts data cannot be null"));

                if (s.UserId == null)
                    return ServiceResult<ShiftsDto>.Failure(
                        new BadRequestException("Please provide the user Id"));

                var checkShifts = await CheckforOpenShift(s.UserId);
                if (checkShifts.IsSuccess)
                {
                    if (checkShifts.Data != null)
                        return ServiceResult<ShiftsDto>.Failure(
                            new BadRequestException("The current User already has an open Shift. Please close it before creating a new one."));
                }

                var strategy = _context.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    return await CreateShiftWithTransactionAsync(s);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating Shift : {ex}", ex);
                return ServiceResult<ShiftsDto>.Failure(
                    new ServerErrorException("Could not create shift."));
            }

        }
        private async Task<ServiceResult<ShiftsDto>> CreateShiftWithTransactionAsync(ShiftsDto s)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Set shift opening details
                    s.DateTimeOpened = DateTime.UtcNow;
                    s.CurrentBalance = 0; //s.OpeningBalance;
                    s.OpeningBalance = s.OpeningBalance;
                    s.DrawerStatus = true;
                    s.Id = "";

                    // Convert to entity and add shift
                    var obj = s.Adapt<tbl_Shift>();
                    var result = await _context.tbl_Shifts.AddAsync(obj);
                    await _context.SaveChangesAsync();

                    // Initiate first transaction for the shift
                    var t = new TransactionDto
                    {
                        SoldBy = s.UserId,
                        ShiftId = result.Entity.Id,
                        TransactionStatus = 0
                    };

                    var y = await _transaction.CreateNewTransaction(t);

                    if (!y.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Error while creating transaction for shift: {Error}", y.Error);
                        return ServiceResult<ShiftsDto>.Failure(y.Error);
                    }

                    var s1 = new ShiftsDto
                    {
                        ShiftEndAmount = 0,
                        DateTimeClosed = null,
                        SubActiveId = null,
                        ShiftEndCash = 0,
                        ShiftEndCheque = 0,
                        CurrentBalance = s.ShiftEndCash,
                        OpeningBalance = s.OpeningBalance,
                        DrawerStatus = s.DrawerStatus,
                        Comment = null,
                        ActiveId = y.Data.Id,
                        Id = (t.ShiftId)
                    };

                    var shiftResult = await UpdateShiftsUsingShiftId(s1);

                    if (!shiftResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Shift created but failed to update shift details: {Error}", shiftResult.Error);
                        return ServiceResult<ShiftsDto>.Failure(
                            new ServerErrorException("Shift created but failed to update shift details. Please contact Admin."));
                    }
                    // Commit the transaction
                    await transaction.CommitAsync();
                    return ServiceResult<ShiftsDto>.Success(shiftResult.Data);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while creating Shift : {ex}", ex);
                    await transaction.RollbackAsync();
                    return ServiceResult<ShiftsDto>.Failure(
                        new ServerErrorException("Could not create shift."));
                }
            }
        }
        #endregion

        #region Read Shifts from Database based on ShiftId
        public async Task<ServiceResult<ShiftsDto>> GetShiftsBasedOnID(string shiftId)
        {

            try
            {
                var result = await _context.tbl_Shifts.FirstOrDefaultAsync(x => x.Id == shiftId);
                await _context.SaveChangesAsync();


                return ServiceResult<ShiftsDto>.Success(result.Adapt<ShiftsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting Shift : {ex}", ex);
                return ServiceResult<ShiftsDto>.Failure(
                    new ServerErrorException("Could not get shift"));
            }

        }
        #endregion

        #region Read active shift from Database based on userid
        public async Task<ServiceResult<ShiftsDto>> GetActiveShiftsforUserperUserId(string userId)
        {
            try
            {
                var result = await _context.tbl_Shifts
                    .Where(x => x.DrawerStatus == true && x.UserId == userId)
                    .OrderByDescending(x => x.DateTimeOpened)
                    .FirstOrDefaultAsync();

                return ServiceResult<ShiftsDto>.Success(result.Adapt<ShiftsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting Shift : {ex}", ex);
                return ServiceResult<ShiftsDto>.Failure(
                    new ServerErrorException($"Could get active shift for user of Id: {userId}"));
            }
        }
        #endregion

        #region Get Active transcationID
        [Obsolete("This method is obsolete. call GetShiftsBasedOnID() directly", false)]
        public async Task<ServiceResult<string>> GetActiveTransactionID(string shiftId)
        {

            var result = await GetShiftsBasedOnID(shiftId);

            return ServiceResult<string>.Success(result.Data?.ActiveId);


        }
        #endregion

        #region update shifts in the  DB
        public async Task<ServiceResult<ShiftsDto>> UpdateShiftsUsingShiftId(ShiftsDto s)
        {
            try
            {
                var objFromDb = await _context.tbl_Shifts.FirstOrDefaultAsync(x => x.Id == s.Id);
                if (objFromDb == null) return ServiceResult<ShiftsDto>.Failure(new NotFoundException($"Shift with Id of {s.Id} not found"));

                objFromDb.ActiveId = s.ActiveId ?? objFromDb.ActiveId;
                objFromDb.Comment = s.Comment ?? objFromDb.Comment;
                objFromDb.CurrentBalance = s.CurrentBalance ?? objFromDb.CurrentBalance;
                objFromDb.OpeningBalance = s.OpeningBalance ?? objFromDb.OpeningBalance;
                objFromDb.ShiftEndBank = s.ShiftEndBank ?? objFromDb.ShiftEndBank;
                objFromDb.ShiftEndCard = s.ShiftEndCard ?? objFromDb.ShiftEndCard;
                objFromDb.ShiftEndCash = s.ShiftEndCash ?? objFromDb.ShiftEndCash;
                objFromDb.DateTimeClosed = s.DateTimeClosed ?? objFromDb.DateTimeClosed;
                objFromDb.ShiftEndCheque = s.ShiftEndCheque ?? objFromDb.ShiftEndCheque;
                objFromDb.DateTimeOpened = s.DateTimeOpened ?? objFromDb.DateTimeOpened;
                objFromDb.DrawerStatus = s.DrawerStatus ?? objFromDb.DrawerStatus;
                objFromDb.ShiftEndAcc = s.ShiftEndAcc ?? objFromDb.ShiftEndAcc;
                objFromDb.SubActiveId = s.SubActiveId ?? objFromDb.SubActiveId;
                objFromDb.UserId = s.UserId ?? objFromDb.UserId;

                await _context.SaveChangesAsync();


                return ServiceResult<ShiftsDto>.Success(objFromDb.Adapt<ShiftsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating Shift : {ex}", ex);
                return ServiceResult<ShiftsDto>.Failure(
                    new ServerErrorException("Could not update shift."));
            }

        }
        #endregion

        #region Close shifts in the  DB

        public async Task<ServiceResult<ShiftsDto>> CloseShiftUsingShiftId(ShiftsDto s)
        {
            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () => await CloseShiftwithSUmmaryAsync(s));

                async Task<ServiceResult<ShiftsDto>> CloseShiftwithSUmmaryAsync(ShiftsDto s)
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {

                        try
                        {
                            var objFromDb = await _context.tbl_Shifts.FirstOrDefaultAsync(x => x.Id == s.Id);
                            if (objFromDb == null) return ServiceResult<ShiftsDto>.Failure(new NotFoundException($"Shift with Id of {s.Id} not found"));

                            if (!(objFromDb.DrawerStatus ?? true)) return ServiceResult<ShiftsDto>.Failure(new BadRequestException($"Shift already closed"));


                            var shiftCloseSummary = s.ShiftclosureSummary.Adapt<List<tbl_ShiftClosureSummary>>();
                            foreach (var item in shiftCloseSummary)
                            {
                                if (string.IsNullOrEmpty(item.PaymentModeID))
                                {
                                    var modeId = await _context.tbl_PaymentModes
                                        .Where(x => x.Description != null && x.Description.ToLower() == item.Description.ToLower())
                                        .Select(x => x.Id)
                                        .FirstOrDefaultAsync();
                                    if (modeId != null) item.PaymentModeID = modeId;
                                }
                            }
                            await _context.tbl_ShiftClosureSummaries.AddRangeAsync(shiftCloseSummary);
                            await _context.SaveChangesAsync();



                            objFromDb.ActiveId = s.ActiveId ?? objFromDb.ActiveId;
                            objFromDb.Comment = s.Comment ?? objFromDb.Comment;
                            objFromDb.CurrentBalance = s.ShiftclosureSummary.Sum(x => x.SaleTotal - x.ShiftExpense) + s.OpeningBalance;
                            objFromDb.DateTimeClosed = DateTime.UtcNow;

                            objFromDb.DrawerStatus = false;


                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            return ServiceResult<ShiftsDto>.Success(objFromDb.Adapt<ShiftsDto>());
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while close Shift : {ex}", ex);
                return ServiceResult<ShiftsDto>.Failure(
                    new ServerErrorException($"Could not close shift of Id: {s.Id}."));
            }




        }
        #endregion

        #region update shifts  in the  DB apart from shift status
        public async Task<ServiceResult<ShiftsDto>> UpdateActiveTransactionInShift(string shiftId, string activateId)
        {
            try
            {
                if (string.IsNullOrEmpty(shiftId) || string.IsNullOrEmpty(activateId))
                    return ServiceResult<ShiftsDto>.Failure(new BadRequestException("Shift Id and activate sale Id are required."));

                var objFromDb = await _context.tbl_Shifts.FirstOrDefaultAsync(x => x.Id == shiftId);
                if (objFromDb == null) return ServiceResult<ShiftsDto>.Failure(new NotFoundException($"Shift with Id of {shiftId} not found"));

                var activeTransaction = await _context.tbl_Transactions.Where(x => x.Id == activateId && x.TransactionStatus < 10).FirstOrDefaultAsync();
                if (activeTransaction == null) return ServiceResult<ShiftsDto>.Failure(new NotFoundException($"No active Transaction with Id of {activateId} was found"));

                var canResume = await CanUserResumeTransactionFromShift(_tenantProvider.GetUserId(), activateId);

                if (!canResume.IsSuccess || !canResume.Data)
                    return ServiceResult<ShiftsDto>.Failure(new BadRequestException("You cannot resume this transaction. It does not belong to any of your shifts."));

                if (!string.IsNullOrEmpty(objFromDb.ActiveId))
                {
                    var currentActiveTransaction = await _context.tbl_Transactions
                        .FirstOrDefaultAsync(x => x.Id == objFromDb.ActiveId);

                    // Only store as SubActiveId if it's still open and has no items
                    if (currentActiveTransaction != null && currentActiveTransaction.TransactionStatus < 10)
                    {
                        var hasItems = await _context.tbl_TransactionDetails
                            .AnyAsync(x => x.TransactionId == objFromDb.ActiveId);

                        if (!hasItems)
                        {
                            objFromDb.SubActiveId = objFromDb.ActiveId;
                            _logger.LogInformation("Stored empty transaction {TransactionId} as SubActiveId", objFromDb.ActiveId);
                        }
                        else
                        {
                            _logger.LogInformation("Current active transaction {TransactionId} has items, not storing as SubActiveId", objFromDb.ActiveId);
                        }
                    }
                }
                objFromDb.ActiveId = activateId;
                //Mark transaction as opened
                activeTransaction.TransactionStatus = (int)statics.SaleStatus.opened;

                await _context.SaveChangesAsync();
                return ServiceResult<ShiftsDto>.Success(objFromDb.Adapt<ShiftsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while activating transaction in Shift : {ex}", ex);
                return ServiceResult<ShiftsDto>.Failure(
                    new ServerErrorException("Could not activate transaction in shift."));
            }
        }
        #endregion

        #region Search Shifts
        public async Task<ServiceResult<PaginationDetails<ShiftsDto>>> SearchShifts(DateTime startDate, DateTime endDate, int offset, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending, string keywords = "", string UserId = "", bool shiftStatus = false)
        {

            IQueryable<tbl_Shift> query = _context.tbl_Shifts.Include(x => x.User).Where(x => x.DateTimeOpened >= startDate && x.DateTimeOpened <= endDate);

            try
            {

                if (!string.IsNullOrEmpty(keywords))
                {

                    query = query.Where(x => (x.Comment != null && x.Comment.Contains(keywords)) ||
                          x.Id.ToString().Contains(keywords));
                }
                if (!string.IsNullOrEmpty(UserId))
                {

                    query = query.Where(x => x.UserId.Contains(UserId));

                }

                if (shiftStatus == true)
                {

                    query = query.Where(x => x.DrawerStatus == true);
                }

                query = query.OrderByDescending(x => x.Id);

                var result = await query.AsNoTracking().ToPaginatedResultAsync(offset, limit, cancellationToken, sortByColumn, sortAscending);
                return ServiceResult<PaginationDetails<ShiftsDto>>.Success(result.Adapt<PaginationDetails<ShiftsDto>>());

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching shifts: {ex}", ex);
                return ServiceResult<PaginationDetails<ShiftsDto>>.Failure(
                    new ServerErrorException("Could not search for shifts."));
            }


        }
        #endregion

        #region search Shifts from Database for combo boxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchShiftsForComboBoxes(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                IQueryable<tbl_Shift> query = _context.tbl_Shifts;

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.Id.ToString().Contains(keywords) ||
                                           c.Comment != null && c.Comment.Contains(keywords));

                }

                var shifts = await query.AsNoTracking()
                                        .Select(x => new ComboBoxDto
                                        {
                                            Id = x.Id,
                                            IdString = x.Id.ToString(),
                                            ValueText = $"{x.DateTimeOpened}_{x.DateTimeClosed}"
                                        })
                                        .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(shifts);

            }

            catch (Exception ex)
            {
                _logger.LogError("Shift matching keywords: {keywords} could not be found. {Error}", keywords, ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(
                    new ServerErrorException($"Error while searching for shifts with keywords : {keywords}"));
            }
        }
        #endregion

        #region Get date of oldest Shift from dataBase
        public async Task<ServiceResult<DateTime?>> GetOldestShiftfromDB()
        {

            try
            {
                var result = await _context.tbl_Shifts
                    .Where(x => x.DateTimeOpened != null)
                    .AsNoTracking()
                    .MinAsync(x => x.DateTimeOpened);


                return ServiceResult<DateTime?>.Success((DateTime)result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching shifts: {ex}", ex);
                return ServiceResult<DateTime?>.Failure(
                    new ServerErrorException("Error searching for old shift"));
            }
        }
        #endregion

        #region Get last transaction for the  Shift from dataBase
        public async Task<ServiceResult<TransactionDto>> GetLastTransactionfromDB(string shiftId)
        {
            try
            {
                var result = await _context.tbl_Transactions
                    .Where(x => x.ShiftId == shiftId && x.TransactionStatus > 9)
                    .OrderByDescending(x => x.TransactionDate)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (result is not null)
                {
                    var output = result.Adapt<TransactionDto>();

                    return ServiceResult<TransactionDto>.Success(output);
                }
                return ServiceResult<TransactionDto>.Failure(new NotFoundException("No completed transaction for this shift"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting last transaction shifts: {ex}", ex);
                return ServiceResult<TransactionDto>.Failure(
                    new ServerErrorException("Could not get last transaction."));
            }
        }
        #endregion

        #region Check if user Has open shift

        public async Task<ServiceResult<ShiftsDto>> CheckforOpenShift(string userId)
        {
            try
            {
                var result = await _context.tbl_Shifts.Where(x => x.DrawerStatus == true).FirstOrDefaultAsync(x => x.UserId == userId);

                if (result == null) return ServiceResult<ShiftsDto>.Failure(new NotFoundException("No open shift exists for user"));
                return ServiceResult<ShiftsDto>.Success(result.Adapt<ShiftsDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting open shift by userId {id}: {ex}", userId, ex);
                return ServiceResult<ShiftsDto>.Failure(
                    new ServerErrorException($"Could not find open shift for user of Id {userId}"));
            }


        }
        #endregion

        #region Check if user can resume a transaction from another user's shift
        public async Task<ServiceResult<bool>> CanUserResumeTransactionFromShift(string userId, string transactionId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(transactionId))
                    return ServiceResult<bool>.Failure(
                        new BadRequestException("User ID and transaction ID are required."));

                // Get the transaction to find its shift
                var transaction = await _context.tbl_Transactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction == null)
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Transaction with ID {transactionId} not found."));

                // Get the shift for this transaction
                var shift = await _context.tbl_Shifts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == transaction.ShiftId);

                if (shift == null)
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Shift with ID {transaction.ShiftId} not found for transaction."));

                // If the shift belongs to the requesting user, they can always resume it
                if (shift.UserId == userId)
                    return ServiceResult<bool>.Success(true);

                // Check if the system allows users to resume transactions from other users' shifts
                bool allowResumeAnyTransaction = false;

                // Retrieve setting from your configuration store (assuming it's in statics.allSettings)
                // You may need to inject a service to access this setting depending on your architecture
                var settingResult = await _context.tbl_Configurations
                    .FirstOrDefaultAsync(c => c.ConfigId == (int)statics.Configurations.AllowUsersResumeAnyTransaction);

                if (settingResult != null)
                {
                    allowResumeAnyTransaction = settingResult.StringValue?.ToLower() == "true";
                }

                // Check if the shift is still open
                var isShiftOpen = shift.DrawerStatus ?? false;

                // Users can resume a transaction if:
                // 1. System allows resuming any transaction (configuration setting)
                // 2. OR it's their own shift (we already checked this above)
                bool canResume = allowResumeAnyTransaction;

                _logger.LogInformation(
                    "User {UserId} attempt to resume transaction {TransactionId} from shift {ShiftId} owned by {ShiftUserId}. " +
                    "Allowed: {CanResume} (AllowResumeAny: {AllowResumeAny}, ShiftOpen: {IsShiftOpen})",
                    userId, transactionId, shift.Id, shift.UserId, canResume, allowResumeAnyTransaction, isShiftOpen);

                return ServiceResult<bool>.Success(canResume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining if user {UserId} can resume transaction {TransactionId}: {Error}",
                    userId, transactionId, ex.Message);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not determine if user can resume transaction."));
            }
        }
        #endregion

    }
}
