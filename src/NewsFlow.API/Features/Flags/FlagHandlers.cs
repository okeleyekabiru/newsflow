using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Features.Flags;

public record ApproveFlagCommand(Guid FlagId, Guid ReviewerId, string? Notes) : IRequest<Result>;
public record RejectFlagCommand(Guid FlagId, Guid ReviewerId, string? Notes) : IRequest<Result>;
public record EscalateFlagCommand(Guid FlagId, Guid ReviewerId, string Notes) : IRequest<Result>;
public record UpdateFlagRuleCommand(
    Guid UserId,
    ArticleCategory Category,
    ContentDecision DefaultDecision,
    string[] TrustedSources,
    string[] BlockedKeywords,
    int SeverityThreshold,
    string? EscalationEmail,
    bool AutoPostTrustedSources) : IRequest<Result>;

public record GetPendingFlagsQuery(Guid UserId) : IRequest<Result<IEnumerable<FlagDto>>>;
public record GetFlagDetailQuery(Guid FlagId, Guid UserId) : IRequest<Result<FlagDetailDto>>;
public record GetFlagRulesQuery(Guid UserId) : IRequest<Result<IEnumerable<FlagRuleDto>>>;

public record FlagDto(
    Guid Id, string ArticleTitle, string FlagReason,
    int SeverityScore, string Category, string[] TriggerKeywords,
    string SourceName, string Status, DateTime CreatedAt);

public record FlagDetailDto(
    Guid Id, string ArticleTitle, string ContentMd, string FlagReason,
    int SeverityScore, string Category, string[] TriggerKeywords,
    string SourceName, string Status, IEnumerable<AuditLogDto> AuditLogs);

public record AuditLogDto(string Action, Guid ActorId, string? Notes, DateTime Timestamp);

public record FlagRuleDto(
    Guid Id, string Category, string DefaultDecision,
    string[] TrustedSources, string[] BlockedKeywords,
    int SeverityThreshold, string? EscalationEmail, bool AutoPostTrustedSources);

public class ApproveFlagHandler : IRequestHandler<ApproveFlagCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IPlatformAdapterFactory _factory;

    public ApproveFlagHandler(IUnitOfWork uow, IPlatformAdapterFactory factory)
    {
        _uow = uow;
        _factory = factory;
    }

    public async Task<Result> Handle(ApproveFlagCommand cmd, CancellationToken ct)
    {
        var flag = await _uow.FlaggedPosts.GetWithAuditLogsAsync(cmd.FlagId, ct);
        if (flag is null) return Result.Failure("Flag not found.");

        var result = flag.Approve(cmd.ReviewerId, cmd.Notes);
        if (result.IsFailure) return result;

        _uow.FlaggedPosts.Update(flag);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}

public class RejectFlagHandler : IRequestHandler<RejectFlagCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public RejectFlagHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(RejectFlagCommand cmd, CancellationToken ct)
    {
        var flag = await _uow.FlaggedPosts.GetWithAuditLogsAsync(cmd.FlagId, ct);
        if (flag is null) return Result.Failure("Flag not found.");

        var result = flag.Reject(cmd.ReviewerId, cmd.Notes);
        if (result.IsFailure) return result;

        _uow.FlaggedPosts.Update(flag);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}

public class EscalateFlagHandler : IRequestHandler<EscalateFlagCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;

    public EscalateFlagHandler(IUnitOfWork uow, IEmailService email)
    {
        _uow = uow;
        _email = email;
    }

    public async Task<Result> Handle(EscalateFlagCommand cmd, CancellationToken ct)
    {
        var flag = await _uow.FlaggedPosts.GetWithAuditLogsAsync(cmd.FlagId, ct);
        if (flag is null) return Result.Failure("Flag not found.");

        var ruleConfig = await _uow.FlagRuleConfigs
            .GetByUserAndCategoryAsync(flag.Article.UserId, flag.Category, ct);

        var result = flag.Escalate(cmd.ReviewerId, cmd.Notes);
        if (result.IsFailure) return result;

        _uow.FlaggedPosts.Update(flag);
        await _uow.CommitAsync(ct);

        if (ruleConfig?.EscalationEmail is not null)
        {
            await _email.SendAsync(
                ruleConfig.EscalationEmail,
                $"[NewsFlow] Escalated flag — {flag.Category} (Severity {flag.SeverityScore}/10)",
                $"Article: {flag.Article.Title}\n\nReason: {flag.FlagReason}\n\nNotes: {cmd.Notes}",
                ct);
        }

        return Result.Success();
    }
}

public class GetPendingFlagsHandler : IRequestHandler<GetPendingFlagsQuery, Result<IEnumerable<FlagDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetPendingFlagsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<FlagDto>>> Handle(GetPendingFlagsQuery query, CancellationToken ct)
    {
        var flags = await _uow.FlaggedPosts.GetByUserIdAsync(query.UserId, ct);
        var pending = flags.Where(f => f.Status == FlagStatus.Pending)
            .OrderByDescending(f => f.SeverityScore)
            .Select(f => new FlagDto(
                f.Id, f.Article.Title, f.FlagReason, f.SeverityScore,
                f.Category.ToString(), f.TriggerKeywords,
                f.SourceName, f.Status.ToString(), f.CreatedAt));

        return Result.Success(pending);
    }
}

public class UpdateFlagRuleHandler : IRequestHandler<UpdateFlagRuleCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateFlagRuleHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateFlagRuleCommand cmd, CancellationToken ct)
    {
        var config = await _uow.FlagRuleConfigs
            .GetByUserAndCategoryAsync(cmd.UserId, cmd.Category, ct);

        if (config is null)
        {
            config = FlagRuleConfig.CreateDefault(cmd.UserId, cmd.Category);
            await _uow.FlagRuleConfigs.AddAsync(config, ct);
        }

        config.Update(cmd.DefaultDecision, cmd.TrustedSources, cmd.BlockedKeywords,
            cmd.SeverityThreshold, cmd.EscalationEmail, cmd.AutoPostTrustedSources);

        _uow.FlagRuleConfigs.Update(config);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}
