using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Features.Accounts;

// ── Commands & Queries ────────────────────────────────────────────────────────

public record GetAccountsQuery(Guid UserId) : IRequest<Result<IEnumerable<AccountDto>>>;

public record ConnectAccountCommand(
    Guid UserId, Platform Platform, string Handle, string AccessToken) : IRequest<Result<Guid>>;

public record DisconnectAccountCommand(Guid AccountId, Guid UserId) : IRequest<Result>;

public record ToggleAccountCommand(Guid AccountId, Guid UserId) : IRequest<Result>;

public record AccountDto(
    Guid Id, string Platform, string Handle,
    bool IsActive, long FollowerCount, decimal MonthlyRevenue, DateTime UpdatedAt);

// ── Handlers ─────────────────────────────────────────────────────────────────

public class GetAccountsHandler : IRequestHandler<GetAccountsQuery, Result<IEnumerable<AccountDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetAccountsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<AccountDto>>> Handle(GetAccountsQuery q, CancellationToken ct)
    {
        var accounts = await _uow.Accounts.GetByUserIdAsync(q.UserId, ct);
        return Result.Success(accounts.Select(a =>
            new AccountDto(a.Id, a.Platform.ToString(), a.Handle,
                a.IsActive, a.FollowerCount, a.MonthlyRevenue, a.UpdatedAt)));
    }
}

public class ConnectAccountHandler : IRequestHandler<ConnectAccountCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    public ConnectAccountHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(ConnectAccountCommand cmd, CancellationToken ct)
    {
        var existing = await _uow.Accounts.GetByPlatformAsync(cmd.UserId, cmd.Platform, ct);
        if (existing is not null)
        {
            existing.UpdateTokens(cmd.AccessToken, null, null);
            _uow.Accounts.Update(existing);
            await _uow.CommitAsync(ct);
            return Result.Success(existing.Id);
        }

        var result = Account.Create(cmd.UserId, cmd.Platform, cmd.Handle, cmd.AccessToken);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _uow.Accounts.AddAsync(result.Value, ct);
        await _uow.CommitAsync(ct);
        return Result.Success(result.Value.Id);
    }
}

public class DisconnectAccountHandler : IRequestHandler<DisconnectAccountCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public DisconnectAccountHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DisconnectAccountCommand cmd, CancellationToken ct)
    {
        var account = await _uow.Accounts.GetByIdAsync(cmd.AccountId, ct);
        if (account is null) return Result.Failure("Account not found.");
        if (account.UserId != cmd.UserId) return Result.Failure("Unauthorized.");

        account.Deactivate();
        _uow.Accounts.Update(account);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}

public class ToggleAccountHandler : IRequestHandler<ToggleAccountCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public ToggleAccountHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(ToggleAccountCommand cmd, CancellationToken ct)
    {
        var account = await _uow.Accounts.GetByIdAsync(cmd.AccountId, ct);
        if (account is null) return Result.Failure("Account not found.");
        if (account.UserId != cmd.UserId) return Result.Failure("Unauthorized.");

        if (account.IsActive) account.Deactivate();
        else account.Activate();

        _uow.Accounts.Update(account);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}
