using AuthNet.Core.Email;

namespace AuthNet.Tests.Integration;

internal sealed class TestEmailSink
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

internal sealed class TestEmailSender(TestEmailSink sink) : IAuthNetEmailSender
{
    public Task SendAsync(AuthNetEmailMessage message, CancellationToken cancellationToken = default)
    {
        sink.Add(message);
        return Task.CompletedTask;
    }
}
