using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using AuthNet.Core.Email;

namespace AuthNet.SampleHost;

public sealed class SampleHostSmtpEmailSender(SampleHostSmtpEmailOptions options) : IAuthNetEmailSender
{
    public Task SendAsync(AuthNetEmailMessage message, CancellationToken cancellationToken = default)
    {
        using var mailMessage = CreateMailMessage(message, options);
        using var client = CreateClient(options);

        return client.SendMailAsync(mailMessage, cancellationToken);
    }

    internal static MailMessage CreateMailMessage(AuthNetEmailMessage message, SampleHostSmtpEmailOptions options)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(options.FromEmail, options.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };
        mailMessage.To.Add(message.To);

        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                message.TextBody,
                null,
                MediaTypeNames.Text.Plain));
        }

        return mailMessage;
    }

#pragma warning disable SYSLIB0014
    private static SmtpClient CreateClient(SampleHostSmtpEmailOptions options)
    {
        var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(options.UserName))
        {
            client.Credentials = new NetworkCredential(options.UserName, options.Password);
        }

        return client;
    }
#pragma warning restore SYSLIB0014
}
