using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFlow.API.Features.Feed;
using NewsFlow.Core.Enums;
using System.Security.Claims;

namespace NewsFlow.API.Controllers;

/// <summary>Article feed — filtered, paginated list of the current user's articles.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private readonly IMediator _mediator;
    public FeedController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// GET /api/feed — paginated article feed with optional status and category filters.
    /// </summary>
    /// <param name="status">Optional status filter (Draft, Published, Archived, …).</param>
    /// <param name="category">Optional category filter (Technology, Politics, …).</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="perPage">Items per page (default 20, max 100).</param>
    [HttpGet]
    public async Task<IActionResult> GetFeed(
        [FromQuery] ArticleStatus? status,
        [FromQuery] ArticleCategory? category,
        [FromQuery] int page    = 1,
        [FromQuery] int perPage = 20,
        CancellationToken ct    = default)
    {
        perPage = Math.Clamp(perPage, 1, 100);
        page    = Math.Max(1, page);

        var result = await _mediator.Send(
            new GetFeedQuery(UserId, status, category, page, perPage), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
