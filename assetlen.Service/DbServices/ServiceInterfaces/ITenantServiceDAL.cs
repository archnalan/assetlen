using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Service.DbServices.ServiceInterfaces
{
    public interface ITenantServiceDAL
    {
        Task<ServiceResult<PaginationDetails<TenantDto>>> GetAllTenants(int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending);
        Task<ServiceResult<TenantDto>> GetTenantById(string tenantId);
        Task<ServiceResult<TenantDto>> CreateTenant(TenantCreateDto tenantDto);
        Task<ServiceResult<TenantDto>> UpdateTenant(string tenantId, TenantDto tenantDto);
        Task<ServiceResult<bool>> DeleteTenant(string tenantId);
    }
}
