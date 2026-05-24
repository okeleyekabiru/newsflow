namespace NewsFlow.Core.Enums;

public enum PostStatus
{
    Draft,
    Pending,
    Scheduled,
    Published,
    Failed,
    Rejected
}

public enum ContentDecision
{
    AutoPost,
    FlagForReview,
    Block
}

public enum FlagStatus
{
    Pending,
    Approved,
    Rejected,
    Escalated
}

public enum ArticleStatus
{
    Draft,
    Published,
    Archived
}

public enum ArticleTemplate
{
    BreakingNews,
    Analysis,
    Opinion,
    Explainer,
    SocialCaption,
    Interview
}
