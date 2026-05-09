using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NumCalc.User.Application.DTOs;
using NumCalc.User.Application.Interfaces.Services;
using NumCalc.User.Infrastructure.Configuration;

namespace NumCalc.User.Infrastructure.Services.EmailSenders;

public class SmtpEmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    private readonly SmtpSettings _settings = options.Value;
    
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port);
        client.EnableSsl = _settings.UseSsl;
        client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

        using var mail = new MailMessage();
        mail.From = new MailAddress(_settings.FromAddress, _settings.FromName);
        mail.Subject = message.Subject;
        mail.To.Add(message.To);

        await client.SendMailAsync(mail, ct);
    }
}