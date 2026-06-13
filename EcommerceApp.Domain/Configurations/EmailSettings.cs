namespace EcommerceApp.Domain.Configurations;

public class EmailSettings
{
    // SMTP Settings
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int Timeout { get; set; } = 10000;

    // Feature Toggles
    public bool EnableEmailVerification { get; set; } = true;
    public bool EnableLoginNotification { get; set; } = true;
    public bool EnableWelcomeEmail { get; set; } = true;

    // Token Expiry (in hours)
    public int EmailVerificationTokenExpiryHours { get; set; } = 24;
    public int PasswordResetTokenExpiryHours { get; set; } = 1;
    public int PhoneVerificationTokenExpiryMinutes { get; set; } = 10;

    // Security Settings
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
}