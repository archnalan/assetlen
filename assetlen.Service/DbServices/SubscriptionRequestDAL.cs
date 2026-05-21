using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.statics;

namespace assetlen.Service.DbServices
{
    public class SubscriptionRequestDAL : ISubscriptionRequestDAL
    {
        private readonly AssetlenDbContext _context;
        private readonly ILogger<SubscriptionRequestDAL> _logger;

        public SubscriptionRequestDAL(AssetlenDbContext context, ILogger<SubscriptionRequestDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────
        // Submit Request (public-facing)
        // ─────────────────────────────────────────────────────

        public async Task<ServiceResult<SubscriptionRequestDto>> SubmitRequest(
            SubscriptionRequestCreateDto dto,
            string? userId,
            string? userName,
            string? userEmail,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = new tbl_SubscriptionRequest
                {
                    OrganisationName = dto.OrganisationName.Trim(),
                    EntityType = dto.EntityType,
                    ContactPersonName = dto.ContactPersonName.Trim(),
                    ContactEmail = dto.ContactEmail.Trim().ToLowerInvariant(),
                    ContactPhone = dto.ContactPhone?.Trim(),
                    Website = dto.Website?.Trim(),
                    Address = dto.Address?.Trim(),
                    RequestedSeats = dto.RequestedSeats,
                    AdditionalNotes = dto.AdditionalNotes,
                    SubmittedByUserId = userId,
                    SubmittedByUserName = userName,
                    SubmittedByEmail = userEmail,
                    Status = SubscriptionRequestStatus.Pending,
                    DateTimeCreated = DateTime.UtcNow
                };

                _context.tbl_SubscriptionRequests.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<SubscriptionRequestDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting enterprise subscription request");
                return ServiceResult<SubscriptionRequestDto>.Failure(
                    new ServerErrorException("Failed to submit subscription request. Please try again."));
            }
        }

        // ─────────────────────────────────────────────────────
        // Read
        // ─────────────────────────────────────────────────────

        public async Task<ServiceResult<SubscriptionRequestDto>> GetById(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_SubscriptionRequests
                    .AsNoTracking()
                    .Include(r => r.Seats.Where(s => s.IsDeleted != true))
                    .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true, cancellationToken);

                if (entity == null)
                    return ServiceResult<SubscriptionRequestDto>.Failure(
                        new NotFoundException("Subscription request not found"));

                return ServiceResult<SubscriptionRequestDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription request {Id}", id);
                return ServiceResult<SubscriptionRequestDto>.Failure(new ServerErrorException("Error loading request"));
            }
        }

        public async Task<ServiceResult<List<SubscriptionRequestDto>>> GetAll(
            SubscriptionRequestQueryDto query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var q = _context.tbl_SubscriptionRequests
                    .AsNoTracking()
                    .Where(r => r.IsDeleted != true);

                // Status filter
                if (!string.IsNullOrEmpty(query.StatusFilter) &&
                    Enum.TryParse<SubscriptionRequestStatus>(query.StatusFilter, out var statusEnum))
                    q = q.Where(r => r.Status == statusEnum);

                // Entity type filter
                if (!string.IsNullOrEmpty(query.EntityTypeFilter) &&
                    Enum.TryParse<EnterpriseEntityType>(query.EntityTypeFilter, out var typeEnum))
                    q = q.Where(r => r.EntityType == typeEnum);

                // Search
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var term = query.SearchTerm.Trim().ToLower();
                    q = q.Where(r =>
                        r.OrganisationName.ToLower().Contains(term) ||
                        r.ContactPersonName.ToLower().Contains(term) ||
                        r.ContactEmail.ToLower().Contains(term) ||
                        (r.ContactPhone != null && r.ContactPhone.Contains(term)));
                }

                // Sort
                q = (query.SortByColumn?.ToLower()) switch
                {
                    "organisationname" => query.SortAscending ? q.OrderBy(r => r.OrganisationName) : q.OrderByDescending(r => r.OrganisationName),
                    "status" => query.SortAscending ? q.OrderBy(r => r.Status) : q.OrderByDescending(r => r.Status),
                    "requestedseats" => query.SortAscending ? q.OrderBy(r => r.RequestedSeats) : q.OrderByDescending(r => r.RequestedSeats),
                    _ => q.OrderByDescending(r => r.DateTimeCreated)
                };

                var list = await q
                    .Skip(query.Offset)
                    .Take(query.Limit)
                    .Include(r => r.Seats.Where(s => s.IsDeleted != true))
                    .ToListAsync(cancellationToken);

                return ServiceResult<List<SubscriptionRequestDto>>.Success(list.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying subscription requests");
                return ServiceResult<List<SubscriptionRequestDto>>.Failure(new ServerErrorException("Error loading requests"));
            }
        }

        public async Task<ServiceResult<SubscriptionRequestStatsDto>> GetStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var requests = await _context.tbl_SubscriptionRequests
                    .AsNoTracking()
                    .Where(r => r.IsDeleted != true)
                    .ToListAsync(cancellationToken);

                var totalSeats = await _context.tbl_SubscriptionSeats
                    .AsNoTracking()
                    .Where(s => s.IsDeleted != true && s.IsActive)
                    .CountAsync(cancellationToken);

                var stats = new SubscriptionRequestStatsDto
                {
                    Total = requests.Count,
                    Pending = requests.Count(r => r.Status == SubscriptionRequestStatus.Pending),
                    UnderReview = requests.Count(r => r.Status == SubscriptionRequestStatus.UnderReview),
                    Quoted = requests.Count(r => r.Status == SubscriptionRequestStatus.Quoted),
                    Active = requests.Count(r => r.Status == SubscriptionRequestStatus.Active),
                    Declined = requests.Count(r => r.Status == SubscriptionRequestStatus.Declined),
                    TotalSeats = totalSeats,
                    TotalRevenue = requests
                        .Where(r => r.Status == SubscriptionRequestStatus.Active && r.QuotedAmount.HasValue)
                        .Sum(r => r.QuotedAmount!.Value)
                };

                return ServiceResult<SubscriptionRequestStatsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription request stats");
                return ServiceResult<SubscriptionRequestStatsDto>.Failure(new ServerErrorException("Error loading stats"));
            }
        }

        // ─────────────────────────────────────────────────────
        // Admin Workflow
        // ─────────────────────────────────────────────────────

        public async Task<ServiceResult<SubscriptionRequestDto>> IssueQuote(
            SubscriptionRequestQuoteDto dto,
            string adminUserId,
            string adminUserName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_SubscriptionRequests
                    .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.IsDeleted != true, cancellationToken);

                if (entity == null)
                    return ServiceResult<SubscriptionRequestDto>.Failure(new NotFoundException("Request not found"));

                entity.QuotedAmount = dto.QuotedAmount;
                entity.QuoteCurrency = dto.QuoteCurrency;
                entity.QuoteNotes = dto.QuoteNotes;
                entity.QuotedDate = DateTime.UtcNow;
                entity.QuotedByUserId = adminUserId;
                entity.QuotedByUserName = adminUserName;
                entity.Status = SubscriptionRequestStatus.Quoted;
                entity.DateTimeModified = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                return ServiceResult<SubscriptionRequestDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing quote for request {Id}", dto.RequestId);
                return ServiceResult<SubscriptionRequestDto>.Failure(new ServerErrorException("Error issuing quote"));
            }
        }

        public async Task<ServiceResult<SubscriptionRequestDto>> ConfirmPayment(
            SubscriptionRequestPaymentDto dto,
            string adminUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_SubscriptionRequests
                    .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.IsDeleted != true, cancellationToken);

                if (entity == null)
                    return ServiceResult<SubscriptionRequestDto>.Failure(new NotFoundException("Request not found"));

                entity.PaymentConfirmedDate = DateTime.UtcNow;
                entity.PaymentReference = dto.PaymentReference;
                entity.PaymentConfirmedByUserId = adminUserId;
                entity.SubscriptionStartDate = dto.SubscriptionStartDate;
                entity.SubscriptionEndDate = dto.SubscriptionEndDate;
                entity.Status = SubscriptionRequestStatus.PaymentConfirmed;
                entity.DateTimeModified = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                return ServiceResult<SubscriptionRequestDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for request {Id}", dto.RequestId);
                return ServiceResult<SubscriptionRequestDto>.Failure(new ServerErrorException("Error confirming payment"));
            }
        }

        public async Task<ServiceResult<SubscriptionRequestDto>> UpdateStatus(
            SubscriptionRequestStatusUpdateDto dto,
            string adminUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.tbl_SubscriptionRequests
                    .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.IsDeleted != true, cancellationToken);

                if (entity == null)
                    return ServiceResult<SubscriptionRequestDto>.Failure(new NotFoundException("Request not found"));

                entity.Status = dto.NewStatus;
                if (!string.IsNullOrEmpty(dto.AdminNotes))
                    entity.AdminNotes = dto.AdminNotes;
                entity.DateTimeModified = DateTime.UtcNow;
                entity.LastModifiedBy = adminUserId;

                if (dto.NewStatus == SubscriptionRequestStatus.Active && entity.SubscriptionStartDate.HasValue)
                {
                    // Activate all seats
                    var seats = await _context.tbl_SubscriptionSeats
                        .Where(s => s.RequestId == dto.RequestId && s.IsDeleted != true)
                        .ToListAsync(cancellationToken);

                    foreach (var seat in seats)
                    {
                        seat.IsActive = true;
                        seat.ActivatedDate = DateTime.UtcNow;
                        seat.ExpiryDate = entity.SubscriptionEndDate;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                return ServiceResult<SubscriptionRequestDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for request {Id}", dto.RequestId);
                return ServiceResult<SubscriptionRequestDto>.Failure(new ServerErrorException("Error updating status"));
            }
        }

        // ─────────────────────────────────────────────────────
        // Seat Management
        // ─────────────────────────────────────────────────────

        public async Task<ServiceResult<SubscriptionSeatDto>> AddSeat(
            SubscriptionSeatCreateDto dto,
            string adminUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await _context.tbl_SubscriptionRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.IsDeleted != true, cancellationToken);

                if (request == null)
                    return ServiceResult<SubscriptionSeatDto>.Failure(new NotFoundException("Request not found"));

                var exists = await _context.tbl_SubscriptionSeats
                    .AnyAsync(s => s.RequestId == dto.RequestId && s.Email == dto.Email.ToLowerInvariant() && s.IsDeleted != true, cancellationToken);

                if (exists)
                    return ServiceResult<SubscriptionSeatDto>.Failure(
                        new BadRequestException($"Seat for {dto.Email} already exists on this subscription."));

                var seat = new tbl_SubscriptionSeat
                {
                    RequestId = dto.RequestId,
                    Email = dto.Email.Trim().ToLowerInvariant(),
                    DisplayName = dto.DisplayName,
                    IsActive = request.Status == SubscriptionRequestStatus.Active,
                    ActivatedDate = request.Status == SubscriptionRequestStatus.Active ? DateTime.UtcNow : null,
                    ExpiryDate = request.SubscriptionEndDate,
                    DateTimeCreated = DateTime.UtcNow,
                    LastModifiedBy = adminUserId
                };

                _context.tbl_SubscriptionSeats.Add(seat);
                await _context.SaveChangesAsync(cancellationToken);

                return ServiceResult<SubscriptionSeatDto>.Success(seat.Adapt<SubscriptionSeatDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding seat to request {Id}", dto.RequestId);
                return ServiceResult<SubscriptionSeatDto>.Failure(new ServerErrorException("Error adding seat"));
            }
        }

        public async Task<ServiceResult<List<SubscriptionSeatDto>>> AddSeatsBulk(
            SubscriptionSeatBulkCreateDto dto,
            string adminUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = await _context.tbl_SubscriptionRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.IsDeleted != true, cancellationToken);

                if (request == null)
                    return ServiceResult<List<SubscriptionSeatDto>>.Failure(new NotFoundException("Request not found"));

                var addedSeats = new List<SubscriptionSeatDto>();

                foreach (var seatDto in dto.Seats)
                {
                    var email = seatDto.Email.Trim().ToLowerInvariant();
                    var exists = await _context.tbl_SubscriptionSeats
                        .AnyAsync(s => s.RequestId == dto.RequestId && s.Email == email && s.IsDeleted != true, cancellationToken);

                    if (exists) continue;

                    var seat = new tbl_SubscriptionSeat
                    {
                        RequestId = dto.RequestId,
                        Email = email,
                        DisplayName = seatDto.DisplayName,
                        IsActive = request.Status == SubscriptionRequestStatus.Active,
                        ActivatedDate = request.Status == SubscriptionRequestStatus.Active ? DateTime.UtcNow : null,
                        ExpiryDate = request.SubscriptionEndDate,
                        DateTimeCreated = DateTime.UtcNow,
                        LastModifiedBy = adminUserId
                    };

                    _context.tbl_SubscriptionSeats.Add(seat);
                    addedSeats.Add(seat.Adapt<SubscriptionSeatDto>());
                }

                await _context.SaveChangesAsync(cancellationToken);
                return ServiceResult<List<SubscriptionSeatDto>>.Success(addedSeats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk adding seats to request {Id}", dto.RequestId);
                return ServiceResult<List<SubscriptionSeatDto>>.Failure(new ServerErrorException("Error adding seats"));
            }
        }

        public async Task<ServiceResult<bool>> RemoveSeat(
            string seatId,
            string adminUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var seat = await _context.tbl_SubscriptionSeats
                    .FirstOrDefaultAsync(s => s.Id == seatId && s.IsDeleted != true, cancellationToken);

                if (seat == null)
                    return ServiceResult<bool>.Failure(new NotFoundException("Seat not found"));

                seat.IsDeleted = true;
                seat.IsActive = false;
                seat.DateTimeModified = DateTime.UtcNow;
                seat.LastModifiedBy = adminUserId;

                await _context.SaveChangesAsync(cancellationToken);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing seat {SeatId}", seatId);
                return ServiceResult<bool>.Failure(new ServerErrorException("Error removing seat"));
            }
        }

        public async Task<ServiceResult<List<SubscriptionSeatDto>>> GetSeatsByRequestId(
            string requestId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var seats = await _context.tbl_SubscriptionSeats
                    .AsNoTracking()
                    .Where(s => s.RequestId == requestId && s.IsDeleted != true)
                    .OrderBy(s => s.Email)
                    .ToListAsync(cancellationToken);

                return ServiceResult<List<SubscriptionSeatDto>>.Success(seats.Select(s => s.Adapt<SubscriptionSeatDto>()).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seats for request {Id}", requestId);
                return ServiceResult<List<SubscriptionSeatDto>>.Failure(new ServerErrorException("Error loading seats"));
            }
        }

        // ─────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────

        private static SubscriptionRequestDto MapToDto(tbl_SubscriptionRequest entity)
        {
            var dto = entity.Adapt<SubscriptionRequestDto>();
            dto.Seats = entity.Seats?
                .Where(s => s.IsDeleted != true)
                .Select(s => s.Adapt<SubscriptionSeatDto>())
                .ToList() ?? new();
            return dto;
        }
    }
}
