namespace AuthNet.Core.Email;

public sealed record AuthNetEmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? TextBody = null);

