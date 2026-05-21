using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.Extensions;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace assetlen.Service.DbServices
{
    public class TenantServiceDAL : ITenantServiceDAL
    {
        private readonly mowtDbContext _context;
        private readonly ILogger<TenantServiceDAL> _logger;

        public TenantServiceDAL(mowtDbContext context, ILogger<TenantServiceDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET ALL Tenants
        public async Task<ServiceResult<PaginationDetails<TenantDto>>> GetAllTenants(int offSet, int limit, CancellationToken cancellationToken, string sortByColumn, bool sortAscending)
        {
            try
            {
                var tenants = await _context.tbl_Tenants.AsNoTracking().ToPaginatedResultAsync(offSet, limit, cancellationToken, sortByColumn, sortAscending);
                var tenantDtos = tenants.Adapt<PaginationDetails<TenantDto>>(); // Using Mapster to convert to DTO
                return ServiceResult<PaginationDetails<TenantDto>>.Success(tenantDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching tenants from database: {Error}", ex);
                return ServiceResult<PaginationDetails<TenantDto>>.Failure(
                    new ServerErrorException("Could not fetch tenants."));
            }
        }

        // GET Tenant by ID
        public async Task<ServiceResult<TenantDto>> GetTenantById(string tenantId)
        {
            try
            {
                var tenant = await _context.tbl_Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    _logger.LogError("Tenant with ID: {TenantId} not found.", tenantId);
                    return ServiceResult<TenantDto>.Failure(
                        new NotFoundException($"Tenant with ID: {tenantId} not found."));
                }

                var tenantDto = tenant.Adapt<TenantDto>();
                return ServiceResult<TenantDto>.Success(tenantDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching tenant by ID {TenantId}: {Error}", tenantId, ex);
                return ServiceResult<TenantDto>.Failure(
                    new ServerErrorException("Could not fetch tenant."));
            }
        }

        // CREATE (POST) a new Tenant
        public async Task<ServiceResult<TenantDto>> CreateTenant(TenantCreateDto tenantDto)
        {
            try
            {
                var tenantEntity = tenantDto.Adapt<tbl_Tenant>();
                tenantEntity.TenantId = Guid.NewGuid().ToString();

                _context.tbl_Tenants.Add(tenantEntity);
                await _context.SaveChangesAsync();

                var createdTenantDto = tenantEntity.Adapt<TenantDto>();
                return ServiceResult<TenantDto>.Success(createdTenantDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while creating tenant: {Error}", ex);
                return ServiceResult<TenantDto>.Failure(
                    new ServerErrorException("Could not create tenant."));
            }
        }

        // UPDATE (PUT) an existing Tenant
        public async Task<ServiceResult<TenantDto>> UpdateTenant(string tenantId, TenantDto tenantDto)
        {
            try
            {
                var existingTenant = await _context.tbl_Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (existingTenant == null)
                {
                    _logger.LogError("Tenant with ID: {TenantId} not found for update.", tenantId);
                    return ServiceResult<TenantDto>.Failure(
                        new NotFoundException($"Tenant with ID: {tenantId} not found."));
                }

                tenantDto.Adapt(existingTenant); // Using Mapster to update entity with DTO values
                _context.tbl_Tenants.Update(existingTenant);
                await _context.SaveChangesAsync();

                var updatedTenantDto = existingTenant.Adapt<TenantDto>();
                return ServiceResult<TenantDto>.Success(updatedTenantDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating tenant with ID {TenantId}: {Error}", tenantId, ex);
                return ServiceResult<TenantDto>.Failure(
                    new ServerErrorException("Could not update tenant."));
            }
        }

        // DELETE a Tenant by ID
        public async Task<ServiceResult<bool>> DeleteTenant(string tenantId)
        {
            try
            {
                var tenant = await _context.tbl_Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);

                if (tenant == null)
                {
                    _logger.LogError("Tenant with ID: {TenantId} not found for deletion.", tenantId);
                    return ServiceResult<bool>.Failure(
                        new NotFoundException($"Tenant with ID: {tenantId} not found."));
                }
                tenant.IsDeleted = true;

                await _context.SaveChangesAsync();

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while deleting tenant with ID {TenantId}: {Error}", tenantId, ex);
                return ServiceResult<bool>.Failure(
                    new ServerErrorException("Could not delete tenant."));
            }
        }
    }
}
