using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFlow.API.Features.Analytics;
using System.Security.Claims;

namespace NewsFlow.API.Controllers;

/// <summary>Query post analytics and revenue data.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/analytics/overview — aggregate stats for the current user.</summary>
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAnalyticsOverviewQuery(UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>GET /api/analytics/posts?postId= — per-post analytics (all posts if omitted).</summary>
    [HttpGet("posts")]
    public async Task<IActionResult> Posts([FromQuery] Guid? postId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPostAnalyticsQuery(UserId, postId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>GET /api/analytics/revenue?from=&to= — total revenue in date range.</summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRevenueQuery(UserId, from, to), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
