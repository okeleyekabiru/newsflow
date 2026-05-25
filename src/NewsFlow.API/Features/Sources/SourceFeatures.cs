using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Features.Sources;

// ── Commands & Queries ────────────────────────────────────────────────────────

public record GetSourcesQuery(Guid UserId) : IRequest<Result<IEnumerable<SourceDto>>>;

public record AddSourceCommand(Guid UserId, string Name, string Url, string Type) : IRequest<Result<Guid>>;

public record RemoveSourceCommand(Guid SourceId, Guid UserId) : IRequest<Result>;

public record TrustSourceCommand(Guid SourceId, Guid UserId) : IRequest<Result>;

public record SourceDto(
    Guid Id, string Name, string Url, string Type,
    bool IsActive, bool IsTrusted, DateTime? LastFetchedAt);

// ── Handlers ─────────────────────────────────────────────────────────────────

public class GetSourcesHandler : IRequestHandler<GetSourcesQuery, Result<IEnumerable<SourceDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetSourcesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<SourceDto>>> Handle(GetSourcesQuery q, CancellationToken ct)
    {
        var sources = await _uow.Sources.GetActiveByUserIdAsync(q.UserId, ct);
        return Result.Success(sources.Select(s =>
            new SourceDto(s.Id, s.Name, s.Url, s.Type, s.IsActive, s.IsTrusted, s.LastFetchedAt)));
    }
}

public class AddSourceHandler : IRequestHandler<AddSourceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    public AddSourceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(AddSourceCommand cmd, CancellationToken ct)
    {
        var result = Source.Create(cmd.UserId, cmd.Name, cmd.Url, cmd.Type);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _uow.Sources.AddAsync(result.Value, ct);
        await _uow.CommitAsync(ct);
        return Result.Success(result.Value.Id);
    }
}

public class RemoveSourceHandler : IRequestHandler<RemoveSourceCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public RemoveSourceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(RemoveSourceCommand cmd, CancellationToken ct)
    {
        var source = await _uow.Sources.GetByIdAsync(cmd.SourceId, ct);
        if (source is null) return Result.Failure("Source not found.");
        if (source.UserId != cmd.UserId) return Result.Failure("Unauthorized.");

        source.Deactivate();
        _uow.Sources.Update(source);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}

public class TrustSourceHandler : IRequestHandler<TrustSourceCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public TrustSourceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(TrustSourceCommand cmd, CancellationToken ct)
    {
        var source = await _uow.Sources.GetByIdAsync(cmd.SourceId, ct);
        if (source is null) return Result.Failure("Source not found.");
        if (source.UserId != cmd.UserId) return Result.Failure("Unauthorized.");

        source.MarkTrusted();
        _uow.Sources.Update(source);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}
