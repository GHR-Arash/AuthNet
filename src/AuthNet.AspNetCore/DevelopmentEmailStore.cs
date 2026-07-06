using AuthNet.Core.Email;
using Microsoft.Extensions.Logging;

namespace AuthNet.AspNetCore;

public sealed class DevelopmentEmailStore
{
    private readonly List<AuthNetEmailMessage> messages = [];

    public IReadOnlyList<AuthNetEmailMessage> Messages
    {
        get
        {
            lock (messages)
            {
                return [.. messages];
            }
        }
    }

    public void Add(AuthNetEmailMessage message)
    {
        lock (messages)
        {
            messages.Add(message);
        }
    }
}

public sealed class DevelopmentAuthNetEmailSender(
    DevelopmentEmailStore store,
    ILogger<DevelopmentAuthNetEmailSender> logger)
    : IAuthNetEmailSender
{
    public Task SendAsync(AuthNetEmailMessage message, CancellationToken cancellationToken = default)
    {
        store.Add(message);
        logger.LogInformation("AuthNet development email to {Recipient}: {Subject}", message.To, message.Subject);
        logger.LogInformation("{HtmlBody}", message.HtmlBody);
        return Task.CompletedTask;
    }
}

