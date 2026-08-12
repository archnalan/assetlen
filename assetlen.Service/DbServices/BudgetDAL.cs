using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

public class BudgetDAL : IBudgetDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<BudgetDAL> _logger;
    private readonly IProjectAccessService _access;

    public BudgetDAL(AssetlenDbContext context, ILogger<BudgetDAL> logger, IProjectAccessService access)
    {
        _context = context;
        _logger = logger;
        _access = access;
    }

    public async Task<ServiceResult<ProjectBudgetSummaryDto>> GetSummary(string projectId, string actingUserId)
    {
        try
        {
            var project = await LoadProjectWithParent(projectId);
            if (project is null)
                return ServiceResult<ProjectBudgetSummaryDto>.Failure(new NotFoundException("Project not found."));
            if (!await _access.CanReadAsync(project, actingUserId))
                return ServiceResult<ProjectBudgetSummaryDto>.Failure(new ForbiddenException("Access denied."));

            var lineItems = await _context.tbl_BudgetLineItems
                .Include(b => b.Stage)
                .Include(b => b.Receipts)
                .Where(b => b.ProjectId == projectId)
                .OrderBy(b => b.Category).ThenBy(b => b.DisplayOrder).ThenBy(b => b.Title)
                .AsNoTracking()
                .ToListAsync();

            var dtos = lineItems.Select(ToLineDto).ToList();
            var summary = new ProjectBudgetSummaryDto
            {
                ProjectId = projectId,
                ProjectBudget = project.TotalBudget ?? 0,
                TotalPlanned = dtos.Sum(l => l.PlannedAmount),
                TotalSpent = dtos.Sum(l => l.TotalSpent),
                PlannedByCategory = dtos.GroupBy(l => l.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(l => l.PlannedAmount)),
                SpentByCategory = dtos.GroupBy(l => l.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(l => l.TotalSpent)),
                LineItems = dtos
            };
            return ServiceResult<ProjectBudgetSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading budget summary for project {ProjectId}", projectId);
            return ServiceResult<ProjectBudgetSummaryDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<BudgetLineItemDto>> AddLineItem(BudgetLineItemCreateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ProjectId))
                return ServiceResult<BudgetLineItemDto>.Failure(new BadRequestException("ProjectId is required."));
            if (string.IsNullOrWhiteSpace(dto.Title))
                return ServiceResult<BudgetLineItemDto>.Failure(new BadRequestException("Title is required."));

            var project = await LoadProjectWithParent(dto.ProjectId);
            if (project is null)
                return ServiceResult<BudgetLineItemDto>.Failure(new NotFoundException("Project not found."));
            if (!await _access.CanManageAsync(project, actingUserId))
                return ServiceResult<BudgetLineItemDto>.Failure(new ForbiddenException("Only the project owner or manager can edit the budget."));

            var nextOrder = await _context.tbl_BudgetLineItems
                .Where(b => b.ProjectId == dto.ProjectId)
                .Select(b => (int?)b.DisplayOrder)
                .MaxAsync() ?? 0;

            var item = new tbl_BudgetLineItem
            {
                ProjectId = dto.ProjectId,
                StageId = dto.StageId,
                Title = dto.Title,
                Notes = dto.Notes,
                Category = dto.Category,
                PlannedAmount = dto.PlannedAmount,
                DisplayOrder = nextOrder + 1,
                CreatedById = actingUserId
            };
            _context.tbl_BudgetLineItems.Add(item);
            await _context.SaveChangesAsync();

            return ServiceResult<BudgetLineItemDto>.Success(ToLineDto(await ReloadLine(item.Id)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding budget line item");
            return ServiceResult<BudgetLineItemDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<BudgetLineItemDto>> UpdateLineItem(BudgetLineItemUpdateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Id))
                return ServiceResult<BudgetLineItemDto>.Failure(new BadRequestException("Id is required."));

            var item = await _context.tbl_BudgetLineItems
                .Include(b => b.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(b => b.Id == dto.Id);
            if (item is null)
                return ServiceResult<BudgetLineItemDto>.Failure(new NotFoundException("Line item not found."));
            if (!await _access.CanManageAsync(item.Project, actingUserId))
                return ServiceResult<BudgetLineItemDto>.Failure(new ForbiddenException("Access denied."));

            if (!string.IsNullOrWhiteSpace(dto.Title)) item.Title = dto.Title;
            if (dto.Notes is not null) item.Notes = dto.Notes;
            if (dto.Category.HasValue) item.Category = dto.Category.Value;
            if (dto.PlannedAmount.HasValue) item.PlannedAmount = dto.PlannedAmount.Value;
            if (dto.StageId is not null) item.StageId = string.IsNullOrEmpty(dto.StageId) ? null : dto.StageId;

            await _context.SaveChangesAsync();
            return ServiceResult<BudgetLineItemDto>.Success(ToLineDto(await ReloadLine(item.Id)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget line item");
            return ServiceResult<BudgetLineItemDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<bool>> DeleteLineItem(string lineItemId, string actingUserId)
    {
        try
        {
            var item = await _context.tbl_BudgetLineItems
                .Include(b => b.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(b => b.Id == lineItemId);
            if (item is null)
                return ServiceResult<bool>.Failure(new NotFoundException("Line item not found."));
            if (!await _access.CanManageAsync(item.Project, actingUserId))
                return ServiceResult<bool>.Failure(new ForbiddenException("Access denied."));

            item.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget line item {Id}", lineItemId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ReceiptDto>> AddReceipt(ReceiptCreateDto dto, string actingUserId)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.BudgetLineItemId))
                return ServiceResult<ReceiptDto>.Failure(new BadRequestException("BudgetLineItemId is required."));

            var line = await _context.tbl_BudgetLineItems
                .Include(b => b.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(b => b.Id == dto.BudgetLineItemId);
            if (line is null)
                return ServiceResult<ReceiptDto>.Failure(new NotFoundException("Line item not found."));
            if (!await _access.CanManageAsync(line.Project, actingUserId))
                return ServiceResult<ReceiptDto>.Failure(new ForbiddenException("Access denied."));

            var receipt = new tbl_Receipt
            {
                BudgetLineItemId = dto.BudgetLineItemId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
                VendorName = dto.VendorName,
                Notes = dto.Notes,
                ReceiptImageUrl = dto.ReceiptImageUrl,
                CreatedById = actingUserId
            };
            _context.tbl_Receipts.Add(receipt);
            await _context.SaveChangesAsync();

            return ServiceResult<ReceiptDto>.Success(await LoadReceiptDto(receipt.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding receipt");
            return ServiceResult<ReceiptDto>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<ReceiptDto>>> GetReceiptsByLineItem(string lineItemId, string actingUserId)
    {
        try
        {
            var line = await _context.tbl_BudgetLineItems
                .Include(b => b.Project)
                    .ThenInclude(p => p!.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == lineItemId);
            if (line is null)
                return ServiceResult<List<ReceiptDto>>.Failure(new NotFoundException("Line item not found."));
            if (!await _access.CanReadAsync(line.Project, actingUserId))
                return ServiceResult<List<ReceiptDto>>.Failure(new ForbiddenException("Access denied."));

            var receipts = await _context.tbl_Receipts
                .Include(r => r.CreatedBy)
                .Include(r => r.BudgetLineItem)
                .Where(r => r.BudgetLineItemId == lineItemId)
                .OrderByDescending(r => r.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            return ServiceResult<List<ReceiptDto>>.Success(receipts.Select(ToReceiptDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing receipts for {LineItemId}", lineItemId);
            return ServiceResult<List<ReceiptDto>>.Failure(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<bool>> DeleteReceipt(string receiptId, string actingUserId)
    {
        try
        {
            var receipt = await _context.tbl_Receipts
                .Include(r => r.BudgetLineItem)
                    .ThenInclude(b => b!.Project)
                        .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(r => r.Id == receiptId);
            if (receipt is null)
                return ServiceResult<bool>.Failure(new NotFoundException("Receipt not found."));
            if (!await _access.CanManageAsync(receipt.BudgetLineItem?.Project, actingUserId))
                return ServiceResult<bool>.Failure(new ForbiddenException("Access denied."));

            receipt.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt {Id}", receiptId);
            return ServiceResult<bool>.Failure(new ServerErrorException(ex.Message));
        }
    }

    // ─── Helpers ──────────────────────────────────────────────

    private Task<tbl_Project?> LoadProjectWithParent(string projectId) =>
        _context.tbl_Projects_RS
            .Include(p => p.ParentProject)
            .FirstOrDefaultAsync(p => p.Id == projectId);

    private async Task<tbl_BudgetLineItem> ReloadLine(string id) =>
        (await _context.tbl_BudgetLineItems
            .Include(b => b.Stage)
            .Include(b => b.Receipts)
            .AsNoTracking()
            .FirstAsync(b => b.Id == id));

    private async Task<ReceiptDto> LoadReceiptDto(string id)
    {
        var r = await _context.tbl_Receipts
            .Include(x => x.CreatedBy)
            .Include(x => x.BudgetLineItem)
            .AsNoTracking()
            .FirstAsync(x => x.Id == id);
        return ToReceiptDto(r);
    }

    private static BudgetLineItemDto ToLineDto(tbl_BudgetLineItem l) => new()
    {
        Id = l.Id,
        ProjectId = l.ProjectId,
        StageId = l.StageId,
        Title = l.Title,
        Notes = l.Notes,
        Category = l.Category,
        PlannedAmount = l.PlannedAmount,
        DisplayOrder = l.DisplayOrder,
        DateTimeCreated = l.DateTimeCreated,
        StageName = l.Stage?.StageName,
        TotalSpent = l.Receipts?.Where(r => !(r.IsDeleted ?? false)).Sum(r => r.Amount) ?? 0,
        ReceiptCount = l.Receipts?.Count(r => !(r.IsDeleted ?? false)) ?? 0
    };

    private static ReceiptDto ToReceiptDto(tbl_Receipt r) => new()
    {
        Id = r.Id,
        BudgetLineItemId = r.BudgetLineItemId,
        Amount = r.Amount,
        PaymentDate = r.PaymentDate,
        VendorName = r.VendorName,
        Notes = r.Notes,
        ReceiptImageUrl = r.ReceiptImageUrl,
        DateTimeCreated = r.DateTimeCreated,
        LineItemTitle = r.BudgetLineItem?.Title,
        CreatedByName = r.CreatedBy is null ? null : $"{r.CreatedBy.FirstName} {r.CreatedBy.LastName}".Trim()
    };
}
