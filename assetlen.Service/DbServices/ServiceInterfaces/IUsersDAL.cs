using assetlen.Service.DataAccess;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface IUsersDAL
    {
        Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchUserByKeywords(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<PaginationDetails<ComboBoxDto>>> SearchUsersForComboBoxes(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
        Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> GetAllUsers(int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
        Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchForUsers(string keywords, int offSet, int limit, string sortByColumn, bool sortAscending, CancellationToken cancellationToken);
        Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchEmployees(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchGeneralUsers(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<PaginationDetails<CreateUserResponseDto>>> SearchDisabledUsers(string keywords, int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);

        // Employee promotion approval flow
        Task<ServiceResult<EmployeeApprovalStatusDto>> SubmitEmployeeApproval(string targetUserId, string approverUserId, string? approverUserName, bool isApproved, string? comment);
        Task<ServiceResult<EmployeeApprovalStatusDto>> GetEmployeeApprovalStatus(string targetUserId, string? callerUserId);
        Task<ServiceResult<PaginationDetails<EmployeeApprovalStatusDto>>> GetPendingPromotions(int offSet, int limit, CancellationToken cancellationToken);
    }
}