using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Features.Settings;

// ── Contracts ────────────────────────────────────────────────────────────────

public record UserSettingsDto(
    string AiModel,
    string OutputLanguage,
    string IngestFrequency,
    string Voice,
    string StockFootage,
    string AspectRatio,
    string EmailAlerts,
    string ReviewAlerts);

public record GetUserSettingsQuery(Guid UserId) : IRequest<Result<UserSettingsDto>>;

public record UpdateUserSettingsCommand(
    Guid UserId,
    string AiModel,
    string OutputLanguage,
    string IngestFrequency,
    string Voice,
    string StockFootage,
    string AspectRatio,
    string EmailAlerts,
    string ReviewAlerts) : IRequest<Result>;

// ── Handlers ─────────────────────────────────────────────────────────────────

public class GetUserSettingsHandler : IRequestHandler<GetUserSettingsQuery, Result<UserSettingsDto>>
{
    private readonly IUnitOfWork _uow;

    public GetUserSettingsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<UserSettingsDto>> Handle(GetUserSettingsQuery query, CancellationToken ct)
    {
        // Fall back to in-memory defaults when the user has never saved settings —
        // no row is created until they explicitly save.
        var s = await _uow.UserSettings.GetByUserIdAsync(query.UserId, ct)
                ?? UserSettings.CreateDefault(query.UserId);

        return Result.Success(new UserSettingsDto(
            s.AiModel, s.OutputLanguage, s.IngestFrequency,
            s.Voice, s.StockFootage, s.AspectRatio,
            s.EmailAlerts, s.ReviewAlerts));
    }
}

public class UpdateUserSettingsHandler : IRequestHandler<UpdateUserSettingsCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateUserSettingsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateUserSettingsCommand cmd, CancellationToken ct)
    {
        var settings = await _uow.UserSettings.GetByUserIdAsync(cmd.UserId, ct);

        if (settings is null)
        {
            // New row: populate then INSERT. Do NOT call repository Update() here —
            // Set.Update() would flip the Added state to Modified and EF would issue an
            // UPDATE against a non-existent row (0 rows → DbUpdateConcurrencyException).
            settings = UserSettings.CreateDefault(cmd.UserId);
            settings.Update(cmd.AiModel, cmd.OutputLanguage, cmd.IngestFrequency,
                cmd.Voice, cmd.StockFootage, cmd.AspectRatio,
                cmd.EmailAlerts, cmd.ReviewAlerts);
            await _uow.UserSettings.AddAsync(settings, ct);
        }
        else
        {
            settings.Update(cmd.AiModel, cmd.OutputLanguage, cmd.IngestFrequency,
                cmd.Voice, cmd.StockFootage, cmd.AspectRatio,
                cmd.EmailAlerts, cmd.ReviewAlerts);
            _uow.UserSettings.Update(settings);
        }

        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}
