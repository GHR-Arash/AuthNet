namespace AuthNet.Core.Email;

public interface IAuthNetEmailSender
{
    Task SendAsync(AuthNetEmailMessage message, CancellationToken cancellationToken = default);
}

