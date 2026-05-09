using NumCalc.User.Application.DTOs;

namespace NumCalc.User.Application.Interfaces.Services;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}