namespace AuthNet.SampleHost;

public sealed class SampleHostSmtpEmailOptions
{
    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}

public static class SampleHostSmtpEmailOptionsValidator
{
    public static SampleHostSmtpEmailOptions GetAndValidate(IConfiguration configuration)
    {
        var options = new SampleHostSmtpEmailOptions();
        configuration.GetSection("AuthNet:Email:Smtp").Bind(options);

        if (!options.Enabled)
        {
            throw new InvalidOperationException("AuthNet sample SMTP email sender requires AuthNet:Email:Smtp:Enabled=true when the development email sender is disabled.");
        }

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new InvalidOperationException("AuthNet sample SMTP email sender requires AuthNet:Email:Smtp:Host.");
        }

        if (options.Port <= 0)
        {
            throw new InvalidOperationException("AuthNet sample SMTP email sender requires AuthNet:Email:Smtp:Port greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail))
        {
            throw new InvalidOperationException("AuthNet sample SMTP email sender requires AuthNet:Email:Smtp:FromEmail.");
        }

        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            throw new InvalidOperationException("AuthNet sample SMTP email sender requires AuthNet:Email:Smtp:FromName.");
        }

        return options;
    }
}
