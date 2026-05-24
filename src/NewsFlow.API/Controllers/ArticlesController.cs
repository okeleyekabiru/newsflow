using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFlow.API.Features.Articles;
using NewsFlow.API.Features.Flags;
using NewsFlow.Core.Enums;
using System.Security.Claims;

namespace NewsFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ArticlesController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetArticlesQuery(UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetArticleQuery(id, UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> GetVersions(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetArticleVersionsQuery(id, UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateArticleCommand(UserId, req.Title, req.ContentMd, req.Category, req.Template), ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
                                : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateArticleCommand(id, UserId, req.Title, req.ContentMd, req.Category), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, [FromBody] PublishArticleRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new PublishArticleCommand(id, UserId, req.AccountIds, req.ScheduledAt), ct);
        return result.IsSuccess ? Ok(new { message = "Article queued for publishing." })
                                : BadRequest(result.Error);
    }
}

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FlagsController : ControllerBase
{
    private readonly IMediator _mediator;
    public FlagsController(IMediator mediator) => _mediator = mediator;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPendingFlagsQuery(UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFlagDetailQuery(id, UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveFlagCommand(id, UserId, req.Notes), ct);
        return result.IsSuccess ? Ok(new { message = "Flag approved. Article will be published." })
                                : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectFlagCommand(id, UserId, req.Notes), ct);
        return result.IsSuccess ? Ok(new { message = "Flag rejected." }) : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/escalate")]
    public async Task<IActionResult> Escalate(Guid id, [FromBody] ReviewRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new EscalateFlagCommand(id, UserId, req.Notes ?? string.Empty), ct);
        return result.IsSuccess ? Ok(new { message = "Flag escalated to senior editor." })
                                : BadRequest(result.Error);
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFlagRulesQuery(UserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("rules")]
    public async Task<IActionResult> UpdateRules([FromBody] UpdateFlagRuleCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd with { UserId = UserId }, ct);
        return result.IsSuccess ? Ok(new { message = "Rules updated." }) : BadRequest(result.Error);
    }
}

public record CreateArticleRequest(string Title, string ContentMd, ArticleCategory Category, ArticleTemplate Template);
public record UpdateArticleRequest(string Title, string ContentMd, ArticleCategory Category);
public record PublishArticleRequest(Guid[] AccountIds, DateTime? ScheduledAt);
public record ReviewRequest(string? Notes);
