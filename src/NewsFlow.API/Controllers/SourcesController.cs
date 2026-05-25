using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFlow.API.Features.Sources;
using System.Security.Claims;

namespace NewsFlow.API.Controllers;

/// <summary>Manage RSS/news feed sources.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly IMediator _mediator;
    public SourcesController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/sources — list all active sources for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSourcesQuery(UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>POST /api/sources — add a new RSS/Atom source.</summary>
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddSourceRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AddSourceCommand(UserId, req.Name, req.Url, req.Type), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, new { id = result.Value })
            : BadRequest(result.Error);
    }

    /// <summary>DELETE /api/sources/{id} — deactivate a source.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveSourceCommand(id, UserId), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>PATCH /api/sources/{id}/trust — mark a source as trusted.</summary>
    [HttpPatch("{id:guid}/trust")]
    public async Task<IActionResult> Trust(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new TrustSourceCommand(id, UserId), ct);
        return result.IsSuccess ? Ok(new { message = "Source marked as trusted." }) : BadRequest(result.Error);
    }
}

public record AddSourceRequest(string Name, string Url, string Type);
