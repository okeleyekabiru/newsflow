using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsFlow.API.Features.AI;
using NewsFlow.Core.Enums;
using System.Security.Claims;

namespace NewsFlow.API.Controllers;

/// <summary>AI-powered content generation endpoints.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    public AIController(IMediator mediator) => _mediator = mediator;

    /// <summary>POST /api/ai/rewrite — rewrite a headline using Claude.</summary>
    [HttpPost("rewrite")]
    public async Task<IActionResult> Rewrite([FromBody] RewriteRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RewriteHeadlineCommand(req.Headline), ct);
        return result.IsSuccess ? Ok(new { headline = result.Value }) : BadRequest(result.Error);
    }

    /// <summary>POST /api/ai/caption — generate a platform-specific caption using Claude.</summary>
    [HttpPost("caption")]
    public async Task<IActionResult> Caption([FromBody] CaptionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateCaptionCommand(req.Content, req.Platform), ct);
        return result.IsSuccess ? Ok(new { caption = result.Value }) : BadRequest(result.Error);
    }

    /// <summary>POST /api/ai/generate — generate a full news article from a topic.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateArticleRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateArticleCommand(req.Topic, req.Category), ct);
        return result.IsSuccess ? Ok(new { contentMd = result.Value }) : BadRequest(result.Error);
    }

    /// <summary>POST /api/ai/script — generate a video script for an existing article.</summary>
    [HttpPost("script")]
    public async Task<IActionResult> Script([FromBody] ScriptRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateScriptCommand(req.ArticleId), ct);
        return result.IsSuccess ? Ok(new { script = result.Value }) : BadRequest(result.Error);
    }
}

public record RewriteRequest(string Headline);
public record CaptionRequest(string Content, Platform Platform);
public record GenerateArticleRequest(string Topic, ArticleCategory Category);
public record ScriptRequest(Guid ArticleId);
