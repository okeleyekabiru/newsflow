using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFlow.API.Features.Settings;
using System.Security.Claims;

namespace NewsFlow.API.Controllers;

/// <summary>User-level preferences (AI, video, notifications).</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SettingsController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/settings — current user's preferences (defaults if unset).</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserSettingsQuery(UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>PUT /api/settings — upsert the current user's preferences.</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserSettingsCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd with { UserId = UserId }, ct);
        return result.IsSuccess ? Ok(new { message = "Settings saved." }) : BadRequest(result.Error);
    }
}
