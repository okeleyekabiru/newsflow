using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFlow.API.Features.Accounts;
using NewsFlow.Core.Enums;
using System.Security.Claims;

namespace NewsFlow.API.Controllers;

/// <summary>Manage connected social-media accounts.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AccountsController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/accounts — list all connected accounts for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAccountsQuery(UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>POST /api/accounts — manually connect an account (API-key flow).</summary>
    [HttpPost]
    public async Task<IActionResult> Connect([FromBody] ConnectAccountRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ConnectAccountCommand(UserId, req.Platform, req.Handle, req.AccessToken), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, new { id = result.Value })
            : BadRequest(result.Error);
    }

    /// <summary>DELETE /api/accounts/{id} — deactivate (soft-delete) an account.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Disconnect(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DisconnectAccountCommand(id, UserId), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>PATCH /api/accounts/{id}/toggle — toggle active/inactive status.</summary>
    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ToggleAccountCommand(id, UserId), ct);
        return result.IsSuccess ? Ok(new { message = "Account toggled." }) : BadRequest(result.Error);
    }
}

public record ConnectAccountRequest(Platform Platform, string Handle, string AccessToken);
