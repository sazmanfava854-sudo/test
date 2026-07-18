using HRPerformance.Application.Common;
using HRPerformance.Application.DTOs.Appeals;
using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Application.Features.Appeals;
public record CreateAppealCommand(CreateAppealRequest Request, Guid EmployeeId, Guid OrganizationId) : IRequest<ApiResponse<AppealDto>>;
public record ReviewAppealCommand(ReviewAppealRequest Request, Guid ReviewerId) : IRequest<ApiResponse<AppealDto>>;
public record GetAppealsQuery(Guid OrganizationId, AppealStatus? Status) : IRequest<ApiResponse<IList<AppealDto>>>;

public class CreateAppealCommandHandler : IRequestHandler<CreateAppealCommand, ApiResponse<AppealDto>>
{
    private readonly IUnitOfWork _uow; private readonly IAuditService _audit;
    public CreateAppealCommandHandler(IUnitOfWork uow, IAuditService audit) { _uow = uow; _audit = audit; }
    public async Task<ApiResponse<AppealDto>> Handle(CreateAppealCommand cmd, CancellationToken ct)
    {
        var appeal = new Appeal { EmployeeId = cmd.EmployeeId, OrganizationId = cmd.OrganizationId, ScoreId = cmd.Request.ScoreId, EvaluationId = cmd.Request.EvaluationId, Reason = cmd.Request.Reason };
        await _uow.Repository<Appeal>().AddAsync(appeal, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Create", "Appeal", appeal.Id.ToString(), ct: ct);
        var emp = await _uow.Repository<Employee>().GetByIdAsync(cmd.EmployeeId, ct);
        return ApiResponse<AppealDto>.Ok(new AppealDto(appeal.Id, appeal.EmployeeId, emp?.FullName ?? "", appeal.Reason, appeal.Status, appeal.CreatedAt, null));
    }
}

public class ReviewAppealCommandHandler : IRequestHandler<ReviewAppealCommand, ApiResponse<AppealDto>>
{
    private readonly IUnitOfWork _uow; private readonly IAuditService _audit;
    public ReviewAppealCommandHandler(IUnitOfWork uow, IAuditService audit) { _uow = uow; _audit = audit; }
    public async Task<ApiResponse<AppealDto>> Handle(ReviewAppealCommand cmd, CancellationToken ct)
    {
        var appeal = await _uow.Repository<Appeal>().Query().Include(a => a.Employee).FirstOrDefaultAsync(a => a.Id == cmd.Request.AppealId, ct);
        if (appeal == null) return ApiResponse<AppealDto>.Fail("اعتراض یافت نشد");
        appeal.Status = cmd.Request.Status; appeal.ReviewedBy = cmd.ReviewerId; appeal.ReviewedAt = DateTime.UtcNow; appeal.ReviewComments = cmd.Request.ReviewComments;
        await _uow.Repository<Appeal>().UpdateAsync(appeal, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.LogAsync("Review", "Appeal", appeal.Id.ToString(), ct: ct);
        return ApiResponse<AppealDto>.Ok(new AppealDto(appeal.Id, appeal.EmployeeId, appeal.Employee?.FullName ?? "", appeal.Reason, appeal.Status, appeal.CreatedAt, appeal.ReviewComments));
    }
}

public class GetAppealsQueryHandler : IRequestHandler<GetAppealsQuery, ApiResponse<IList<AppealDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetAppealsQueryHandler(IUnitOfWork uow) => _uow = uow;
    public async Task<ApiResponse<IList<AppealDto>>> Handle(GetAppealsQuery q, CancellationToken ct)
    {
        var query = _uow.Repository<Appeal>().Query().Include(a => a.Employee).Where(a => a.OrganizationId == q.OrganizationId);
        if (q.Status.HasValue) query = query.Where(a => a.Status == q.Status);
        var items = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
        return ApiResponse<IList<AppealDto>>.Ok(items.Select(a => new AppealDto(a.Id, a.EmployeeId, a.Employee?.FullName ?? "", a.Reason, a.Status, a.CreatedAt, a.ReviewComments)).ToList());
    }
}
