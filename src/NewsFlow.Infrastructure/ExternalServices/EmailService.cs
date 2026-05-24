using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ExternalServices;

/// <summary>
/// SMTP email sender.  Configure via appsettings:
/// <code>
/// "Email": {
///   "Host": "smtp.example.com",
///   "Port": 587,
///   "UseSsl": true,
///   "Username": "user@example.com",
///   "Password": "...",
///   "FromAddress": "noreply@example.com",
///   "FromName": "NewsFlow"
/// }
/// </code>
/// In development, set "Email:Provider": "Console" to skip SMTP and
/// write the message to the log instead.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        // Development shortcut — log the email instead of sending it
        if (string.Equals(_config["Email:Provider"], "Console", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "[EmailService - Console] To: {To} | Subject: {Subject}\n{Body}",
                to, subject, body);
            return;
        }

        var host     = _config["Email:Host"]        ?? throw new InvalidOperationException("Email:Host is not configured.");
        var port     = int.Parse(_config["Email:Port"] ?? "587");
        var useSsl   = bool.Parse(_config["Email:UseSsl"] ?? "true");
        var userName = _config["Email:Username"]    ?? throw new InvalidOperationException("Email:Username is not configured.");
        var password = _config["Email:Password"]    ?? throw new InvalidOperationException("Email:Password is not configured.");
        var from     = _config["Email:FromAddress"] ?? userName;
        var fromName = _config["Email:FromName"]    ?? "NewsFlow";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl   = useSsl,
            Credentials = new NetworkCredential(userName, password),
        };

        using var message = new MailMessage
        {
            From    = new MailAddress(from, fromName),
            Subject = subject,
            Body    = body,
        };
        message.To.Add(to);

        await client.SendMailAsync(message, ct);

        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }
}
