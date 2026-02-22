using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace InsureX.Application.Services;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendBulkEmailAsync(List<string> recipients, string subject, string body, bool useBcc = true);
}

public class NotificationService : INotificationService
{
    private readonly SmtpSettings _smtpSettings;

    public NotificationService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            EnableSsl = _smtpSettings.EnableSsl,
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
        };

        var message = new MailMessage(_smtpSettings.From, to, subject, body)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
    }

    public async Task SendBulkEmailAsync(List<string> recipients, string subject, string body, bool useBcc = true)
    {
        using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            EnableSsl = _smtpSettings.EnableSsl,
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
        };

        var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        if (useBcc)
        {
            foreach (var recipient in recipients)
            {
                message.Bcc.Add(recipient);
            }
        }
        else
        {
            foreach (var recipient in recipients)
            {
                message.To.Add(recipient);
            }
        }

        await client.SendMailAsync(message);
    }
}

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string From { get; set; }
}