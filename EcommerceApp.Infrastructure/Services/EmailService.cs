using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EcommerceApp.Application.DTOs.Email;
using EcommerceApp.Application.Interfaces.Services;
using EcommerceApp.Application.Services;
using EcommerceApp.Domain.Configurations;

namespace EcommerceApp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(EmailMessage message)
    {
        try
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                Timeout = _emailSettings.Timeout
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml
            };

            mailMessage.To.Add(new MailAddress(message.To, message.ToName));

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Email}", message.To);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", message.To);
            return false;
        }
    }

    public async Task<bool> SendVerificationEmailAsync(string email, string fullName, string verificationLink)
    {
        var body = EmailTemplateService.GetVerificationEmailBody(fullName, verificationLink);

        var message = new EmailMessage
        {
            To = email,
            ToName = fullName,
            Subject = "Verify Your Email Address - EcommerceApp",
            Body = body,
            IsHtml = true
        };

        return await SendEmailAsync(message);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string resetLink)
    {
        var body = EmailTemplateService.GetPasswordResetEmailBody(fullName, resetLink);

        var message = new EmailMessage
        {
            To = email,
            ToName = fullName,
            Subject = "Reset Your Password - EcommerceApp",
            Body = body,
            IsHtml = true
        };

        return await SendEmailAsync(message);
    }
}