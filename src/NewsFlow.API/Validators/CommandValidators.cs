using FluentValidation;
using NewsFlow.API.Features.Articles;
using NewsFlow.API.Features.Flags;

namespace NewsFlow.API.Validators;

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.ContentMd)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

public class UpdateArticleCommandValidator : AbstractValidator<UpdateArticleCommand>
{
    public UpdateArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.ContentMd)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Article ID is required.");
    }
}

public class PublishArticleCommandValidator : AbstractValidator<PublishArticleCommand>
{
    public PublishArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Article ID is required.");

        RuleFor(x => x.AccountIds)
            .NotEmpty().WithMessage("At least one account must be selected.")
            .Must(ids => ids.Length > 0).WithMessage("At least one account is required.");

        RuleFor(x => x.ScheduledAt)
            .Must(d => d == null || d > DateTime.UtcNow)
            .WithMessage("Scheduled time must be in the future.");
    }
}

public class ApproveFlagCommandValidator : AbstractValidator<ApproveFlagCommand>
{
    public ApproveFlagCommandValidator()
    {
        RuleFor(x => x.FlagId).NotEmpty();
        RuleFor(x => x.ReviewerId).NotEmpty();
    }
}

public class EscalateFlagCommandValidator : AbstractValidator<EscalateFlagCommand>
{
    public EscalateFlagCommandValidator()
    {
        RuleFor(x => x.FlagId).NotEmpty();
        RuleFor(x => x.ReviewerId).NotEmpty();
        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Escalation notes are required.")
            .MinimumLength(10).WithMessage("Please provide more detail in your escalation notes.");
    }
}

public class UpdateFlagRuleCommandValidator : AbstractValidator<UpdateFlagRuleCommand>
{
    public UpdateFlagRuleCommandValidator()
    {
        RuleFor(x => x.SeverityThreshold)
            .InclusiveBetween(1, 10)
            .WithMessage("Severity threshold must be between 1 and 10.");

        RuleFor(x => x.EscalationEmail)
            .EmailAddress().WithMessage("Escalation email must be a valid email address.")
            .When(x => !string.IsNullOrEmpty(x.EscalationEmail));
    }
}
