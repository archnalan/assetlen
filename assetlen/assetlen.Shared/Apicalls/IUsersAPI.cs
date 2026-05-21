
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.Users;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Apicalls
{
    public interface IUsersAPI
    {
        [Get("/api/Users/SearchUsersForComboBoxes")]
        Task<IApiResponse<PaginationDetails<ComboBoxDto>>> SearchUsersForComboBoxes([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Users/SearchUserByKeywords")]
        Task<IApiResponse<PaginationDetails<CreateUserResponseDto>>> SearchUserByKeywords([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Users/GetAllEmployees")]
        Task<IApiResponse<PaginationDetails<AppUserDto>>> GetAllEmployees([Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Users/SearchForEmployees")]
        Task<IApiResponse<PaginationDetails<AppUserDto>>> SearchForEmployees([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Users/SearchEmployeeUsers")]
        Task<IApiResponse<PaginationDetails<CreateUserResponseDto>>> SearchEmployeeUsers([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Users/SearchGeneralUsers")]
        Task<IApiResponse<PaginationDetails<CreateUserResponseDto>>> SearchGeneralUsers([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Users/SearchDisabledUsers")]
        Task<IApiResponse<PaginationDetails<CreateUserResponseDto>>> SearchDisabledUsers([Query] string keywords, [Query] int offSet = 0, [Query] int limit = 12, [Query] string? sortByColumn = null, [Query] bool sortAscending = false, [Query] CancellationToken cancellationToken = default);

        [Get("/api/Users/GetEmployerDomain")]
        Task<IApiResponse<string>> GetEmployerDomain();

        // Employee Promotion
        [Post("/api/Users/SubmitEmployeeApproval")]
        Task<IApiResponse<EmployeeApprovalStatusDto>> SubmitEmployeeApproval([Body] SubmitEmployeeApprovalDto dto);

        [Get("/api/Users/GetEmployeeApprovalStatus")]
        Task<IApiResponse<EmployeeApprovalStatusDto>> GetEmployeeApprovalStatus([Query] string targetUserId);

        [Get("/api/Users/GetPendingPromotions")]
        Task<IApiResponse<PaginationDetails<EmployeeApprovalStatusDto>>> GetPendingPromotions([Query] int offSet = 0, [Query] int limit = 30, [Query] CancellationToken cancellationToken = default);
    }
}
