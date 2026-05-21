using mowt.Service.DataAccess;
using mowt.Service.Extensions;
using mowt.ServiceHandler;
using mowt.Shared.Models.Models;
using mowt.Shared.Models.Models.ViewModels;
using Mapster;
using mowt.Shared.Models.Models.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mowt.Service.DbServices.ServiceInterfaces;
using Microsoft.Extensions.Configuration;

namespace mowt.Service.DbServices
{
    public class UsersDAL : IUsersDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<ProductsDAL> _logger;
        private readonly IConfiguration _config;
        public UsersDAL(mowtDbContext context, ILogger<ProductsDAL> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }
        #region Search Users for combo boxes
        public async Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchUsersForComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
        {

            IQueryable<AppUser> query = _context.Users;
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.
                             Where(x => x.FirstName.ToString() == keywords ||
                             x.LastName.Contains(keywords) ||
                             x.UserName != null && x.UserName.Contains(keywords) ||
                             x.Email != null && x.Email.Contains(keywords) ||
                             x.Address != null && x.Address.Contains(keywords) ||
                             x.Aboutme != null && x.Aboutme.Contains(keywords) ||
                             x.Contacts != null && x.Contacts.Contains(keywords)
                             );
                }

                var result = await query.AsNoTracking().Select(x => new ComboBoxDto
                {

                    Id = x.Id,
                    IdString = x.Id,
                    ValueText = $"{x.FirstName} {x.LastName}"
                }).ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                return ServiceResult<PaginationDetails<ComboBoxDto>>.Success(result);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching Users: {ex}", ex);
                return ServiceResult<PaginationDetails<ComboBoxDto>>.Failure(new ServerErrorException("Could not search for users."));
            }
        }
        #endregion

        #region search Users from Database based On Keywords
        public async Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchUserByKeywords(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var query = _context.Users.AsNoTracking();
                if (string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.FirstName.Contains(keywords) ||
                                            c.LastName.Contains(keywords) ||
                                            c.Email != null && c.Email.Contains(keywords));
                }

                var users = await query
                .ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var result = users.Adapt<PaginationDetails<CreateUserResponseDto>>();

                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Success(result);

            }

            catch (Exception ex)
            {
                _logger.LogError("Customer matching keywords: {keywords} could not found.{Error}", keywords, ex);
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Failure(
                    new ServerErrorException($"Error while searching user. Please contact Admin"));
            }
        }
        #endregion

        public async Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> GetAllUsers(int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
        {
            IQueryable<AppUser> query = _context.Users;
            try
            {
                var result = await query.AsNoTracking().ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                var output = result.Adapt<PaginationDetails<CreateUserResponseDto>>();
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Success(output);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting all Users: {ex}", ex);
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Failure(new ServerErrorException(ex.Message));
            }
        }

        public async Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchForUsers(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken)
        {
            IQueryable<AppUser> query = _context.Users;
            try
            {
                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.
                             Where(x => x.FirstName.ToString() == keywords ||
                             x.LastName.Contains(keywords) ||
                             x.UserName != null && x.UserName.Contains(keywords) ||
                             x.Email != null && x.Email.Contains(keywords) ||
                             x.Address != null && x.Address.Contains(keywords) ||
                             x.Aboutme != null && x.Aboutme.Contains(keywords) ||
                             x.Contacts != null && x.Contacts.Contains(keywords)
                             );
                }
                var result = await query.AsNoTracking().ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                var output = result.Adapt<PaginationDetails<CreateUserResponseDto>>();
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Success(output);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching Users: {ex}", ex);
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Failure(new ServerErrorException(ex.Message));
            }
        }

        #region Search Employees (by IsEmployee flag)
        public async Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchEmployees(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var query = _context.Users.AsNoTracking()
                    .Where(u => u.IsEmployee == true)
                    .Where(u => u.IsDeleted != true);

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.FirstName.Contains(keywords) ||
                                             c.LastName.Contains(keywords) ||
                                             c.Email != null && c.Email.Contains(keywords) ||
                                             c.UserName != null && c.UserName.Contains(keywords));
                }

                var users = await query.ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var result = users.Adapt<PaginationDetails<CreateUserResponseDto>>();

                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching Employees: {ex}", ex);
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Failure(
                    new ServerErrorException("Error while searching employees. Please contact Admin"));
            }
        }
        #endregion

        #region Search General Users (non-employee, active, not deleted)
        public async Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchGeneralUsers(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var query = _context.Users.AsNoTracking()
                    .Where(u => u.IsDeleted != true)
                    .Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow)
                    .Where(u => u.IsEmployee != true);

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.FirstName.Contains(keywords) ||
                                             c.LastName.Contains(keywords) ||
                                             c.Email != null && c.Email.Contains(keywords) ||
                                             c.UserName != null && c.UserName.Contains(keywords));
                }

                var users = await query.ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);

                var result = users.Adapt<PaginationDetails<CreateUserResponseDto>>();
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching General Users: {ex}", ex);
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Failure(
                    new ServerErrorException("Error while searching general users. Please contact Admin"));
            }
        }
        #endregion

        #region Search Disabled/Deleted Users
        public async Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchDisabledUsers(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var query = _context.Users.AsNoTracking()
                    .Where(u => u.IsDeleted == true ||
                                (u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow));

                if (!string.IsNullOrEmpty(keywords))
                {
                    query = query.Where(c => c.FirstName.Contains(keywords) ||
                                             c.LastName.Contains(keywords) ||
                                             c.Email != null && c.Email.Contains(keywords) ||
                                             c.UserName != null && c.UserName.Contains(keywords));
                }

                var users = await query.ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                var result = users.Adapt<PaginationDetails<CreateUserResponseDto>>();

                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching Disabled Users: {ex}", ex);
                return ServiceResult<PaginationDetails<CreateUserResponseDto>>.Failure(
                    new ServerErrorException("Error while searching disabled users. Please contact Admin"));
            }
        }
        #endregion

        #region Employee Promotion Approval Flow

        /// <summary>
        /// Submits an admin's approval or rejection for a user's promotion to employee.
        /// When 2+ approvals exist the user's IsEmployee flag is automatically set to true.
        /// </summary>
        public async Task<ServiceResult<EmployeeApprovalStatusDto>> SubmitEmployeeApproval(
            string targetUserId, string approverUserId, string? approverUserName,
            bool isApproved, string? comment)
        {
            try
            {
                var targetUser = await _context.Users.FindAsync(targetUserId);
                if (targetUser == null)
                    return ServiceResult<EmployeeApprovalStatusDto>.Failure(new NotFoundException("Target user not found."));

                // Remove any existing vote from this approver for this user (idempotent re-vote)
                var existingVote = await _context.tbl_EmployeeApprovals
                    .FirstOrDefaultAsync(a => a.TargetUserId == targetUserId && a.ApproverUserId == approverUserId);

                if (existingVote != null)
                {
                    existingVote.IsApproved = isApproved;
                    existingVote.Comment = comment;
                    existingVote.ApprovedAt = DateTime.UtcNow;
                }
                else
                {
                    var approval = new tbl_EmployeeApproval
                    {
                        TargetUserId = targetUserId,
                        ApproverUserId = approverUserId,
                        ApproverUserName = approverUserName,
                        IsApproved = isApproved,
                        Comment = comment,
                        ApprovedAt = DateTime.UtcNow
                    };
                    await _context.tbl_EmployeeApprovals.AddAsync(approval);
                }

                await _context.SaveChangesAsync();

                // Count approvals
                var allVotes = await _context.tbl_EmployeeApprovals
                    .Where(a => a.TargetUserId == targetUserId)
                    .ToListAsync();

                var approvalCount = allVotes.Count(v => v.IsApproved);
                var rejectionCount = allVotes.Count(v => !v.IsApproved);

                // If 2+ approvals reached and user is not yet an employee, promote them
                if (approvalCount >= 2 && !targetUser.IsEmployee)
                {
                    targetUser.IsEmployee = true;
                    await _context.SaveChangesAsync();
                }

                var status = new EmployeeApprovalStatusDto
                {
                    TargetUserId = targetUserId,
                    TargetUserName = targetUser.FullName,
                    ApprovalCount = approvalCount,
                    RejectionCount = rejectionCount,
                    IsPromoted = targetUser.IsEmployee,
                    CurrentAdminHasVoted = true,
                    Approvals = allVotes.Select(v => new EmployeeApprovalEntryDto
                    {
                        Id = v.Id,
                        ApproverUserId = v.ApproverUserId,
                        ApproverUserName = v.ApproverUserName,
                        IsApproved = v.IsApproved,
                        Comment = v.Comment,
                        ApprovedAt = v.ApprovedAt
                    }).ToList()
                };

                return ServiceResult<EmployeeApprovalStatusDto>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while submitting employee approval: {ex}", ex);
                return ServiceResult<EmployeeApprovalStatusDto>.Failure(
                    new ServerErrorException("Error while submitting employee approval. Please contact Admin"));
            }
        }

        /// <summary>
        /// Gets the current approval status for a user's promotion request.
        /// </summary>
        public async Task<ServiceResult<EmployeeApprovalStatusDto>> GetEmployeeApprovalStatus(
            string targetUserId, string? callerUserId)
        {
            try
            {
                var targetUser = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == targetUserId);
                if (targetUser == null)
                    return ServiceResult<EmployeeApprovalStatusDto>.Failure(new NotFoundException("Target user not found."));

                var allVotes = await _context.tbl_EmployeeApprovals.AsNoTracking()
                    .Where(a => a.TargetUserId == targetUserId)
                    .ToListAsync();

                var approvalCount = allVotes.Count(v => v.IsApproved);
                var rejectionCount = allVotes.Count(v => !v.IsApproved);
                var callerVoted = callerUserId != null && allVotes.Any(v => v.ApproverUserId == callerUserId);

                var status = new EmployeeApprovalStatusDto
                {
                    TargetUserId = targetUserId,
                    TargetUserName = targetUser.FullName,
                    ApprovalCount = approvalCount,
                    RejectionCount = rejectionCount,
                    IsPromoted = targetUser.IsEmployee,
                    CurrentAdminHasVoted = callerVoted,
                    Approvals = allVotes.Select(v => new EmployeeApprovalEntryDto
                    {
                        Id = v.Id,
                        ApproverUserId = v.ApproverUserId,
                        ApproverUserName = v.ApproverUserName,
                        IsApproved = v.IsApproved,
                        Comment = v.Comment,
                        ApprovedAt = v.ApprovedAt
                    }).ToList()
                };

                return ServiceResult<EmployeeApprovalStatusDto>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting employee approval status: {ex}", ex);
                return ServiceResult<EmployeeApprovalStatusDto>.Failure(
                    new ServerErrorException("Could not retrieve employee approval status."));
            }
        }

        /// <summary>
        /// Returns all general users who have at least one pending approval vote (not yet promoted).
        /// </summary>
        public async Task<ServiceResult<PaginationDetails<EmployeeApprovalStatusDto>>> GetPendingPromotions(
            int offSet, int limit, CancellationToken cancellationToken)
        {
            try
            {
                // Users who have approval entries but IsEmployee is still false
                var pendingUserIds = await _context.tbl_EmployeeApprovals.AsNoTracking()
                    .Where(a => a.IsApproved)
                    .Select(a => a.TargetUserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var users = await _context.Users.AsNoTracking()
                    .Where(u => pendingUserIds.Contains(u.Id) && u.IsEmployee != true && u.IsDeleted != true)
                    .ToListAsync(cancellationToken);

                var result = new List<EmployeeApprovalStatusDto>();
                foreach (var user in users)
                {
                    var votes = await _context.tbl_EmployeeApprovals.AsNoTracking()
                        .Where(a => a.TargetUserId == user.Id)
                        .ToListAsync(cancellationToken);

                    result.Add(new EmployeeApprovalStatusDto
                    {
                        TargetUserId = user.Id,
                        TargetUserName = user.FullName,
                        ApprovalCount = votes.Count(v => v.IsApproved),
                        RejectionCount = votes.Count(v => !v.IsApproved),
                        IsPromoted = user.IsEmployee
                    });
                }

                var paged = new PaginationDetails<EmployeeApprovalStatusDto>
                {
                    Data = result.Skip(offSet).Take(limit).ToList(),
                    TotalSize = result.Count,
                    OffSet = offSet,
                    Limit = limit
                };

                return ServiceResult<PaginationDetails<EmployeeApprovalStatusDto>>.Success(paged);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while getting pending promotions: {ex}", ex);
                return ServiceResult<PaginationDetails<EmployeeApprovalStatusDto>>.Failure(
                    new ServerErrorException("Could not retrieve pending promotions."));
            }
        }
        #endregion
    }
}
