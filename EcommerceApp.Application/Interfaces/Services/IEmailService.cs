using EcommerceApp.Application.DTOs.Email;

namespace EcommerceApp.Application.Interfaces.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailMessage message);
    Task<bool> SendVerificationEmailAsync(string email, string fullName, string verificationLink);
    Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string resetLink);
}