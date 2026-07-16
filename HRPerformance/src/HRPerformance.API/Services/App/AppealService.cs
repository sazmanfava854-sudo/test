using HRPerformance.Common;
using HRPerformance.DTOs.Appeals;
using HRPerformance.Entities;
using HRPerformance.Enums;
using HRPerformance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Services.App;

public class AppealService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public AppealService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<ApiResponse<IList<AppealDto>>> GetAllAsync(Guid organizationId, AppealStatus? status, CancellationToken ct = default)
    {
        var query = _uow.Repository<Appeal>().Query().Include(a => a.Employee).Where(a => a.OrganizationId == organizationId);
        if (status.HasValue) query = query.Where(a => a.Status == status);
        var items = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
        return ApiResponse<IList<AppealDto>>.Ok(items.Select(a =>
            new AppealDto(a.Id, a.EmployeeId, a.Employee?.FullName ?? "", a.Reason, a.Status, a.CreatedAt, a.ReviewComments)).ToList());
    }

    public async Task<ApiResponse<AppealDto>> CreateAsync(CreateAppealRequest request, Guid employeeId, Guid organizationId, CancellationToken ct = default)
    {
        var appeal = new Appeal
        {
            EmployeeId = employeeId,
            OrganizationId = organizationId,
            ScoreId = request.ScoreId,
            EvaluationId = request.EvaluationId,
            Reason = request.Reason
        };
        await _uow.Repository<Appeal>().AddAsync(appeal, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", "Appeal", appeal.Id.ToString(), ct: ct);
        var emp = await _uow.Repository<Employee>().GetByIdAsync(employeeId, ct);
        return ApiResponse<AppealDto>.Ok(new AppealDto(appeal.Id, appeal.EmployeeId, emp?.FullName ?? "", appeal.Reason, appeal.Status, appeal.CreatedAt, null));
    }

    public async Task<ApiResponse<AppealDto>> ReviewAsync(ReviewAppealRequest request, Guid reviewerId, CancellationToken ct = default)
    {
        var appeal = await _uow.Repository<Appeal>().Query().Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == request.AppealId, ct);
        if (appeal == null) return ApiResponse<AppealDto>.Fail("اعتراض یافت نشد");

        appeal.Status = request.Status;
        appeal.ReviewedBy = reviewerId;
        appeal.ReviewedAt = DateTime.UtcNow;
        appeal.ReviewComments = request.ReviewComments;
        await _uow.Repository<Appeal>().UpdateAsync(appeal, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Review", "Appeal", appeal.Id.ToString(), ct: ct);
        return ApiResponse<AppealDto>.Ok(new AppealDto(appeal.Id, appeal.EmployeeId, appeal.Employee?.FullName ?? "", appeal.Reason, appeal.Status, appeal.CreatedAt, appeal.ReviewComments));
    }
}
