namespace EcommerceApp.Application.Services;

public static class EmailTemplateService
{
    public static string GetVerificationEmailBody(string fullName, string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Verify Your Email</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background-color: #4CAF50; padding: 20px; text-align: center;'>
            <h1 style='color: white;'>Verify Your Email</h1>
        </div>
        <div style='padding: 20px;'>
            <h2>Hello {fullName},</h2>
            <p>Please click the button below to verify your email address:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{verificationLink}' 
                   style='background-color: #4CAF50; color: white; padding: 12px 24px; 
                          text-decoration: none; border-radius: 4px;'>
                    Verify Email
                </a>
            </div>
            <p>Or copy this link: <br/>{verificationLink}</p>
            <p>This link expires in 24 hours.</p>
            <hr>
            <p style='font-size: 12px;'>If you didn't create an account, ignore this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetPasswordResetEmailBody(string fullName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Reset Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background-color: #ff9800; padding: 20px; text-align: center;'>
            <h1 style='color: white;'>Reset Your Password</h1>
        </div>
        <div style='padding: 20px;'>
            <h2>Hello {fullName},</h2>
            <p>We received a request to reset your password. Click below:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}' 
                   style='background-color: #ff9800; color: white; padding: 12px 24px; 
                          text-decoration: none; border-radius: 4px;'>
                    Reset Password
                </a>
            </div>
            <p>Or copy: {resetLink}</p>
            <p>This link expires in 1 hour.</p>
            <p>If you didn't request this, ignore this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetWelcomeEmailBody(string fullName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Welcome!</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background-color: #4CAF50; padding: 20px; text-align: center;'>
            <h1 style='color: white;'>Welcome to EcommerceApp!</h1>
        </div>
        <div style='padding: 20px;'>
            <h2>Hello {fullName},</h2>
            <p>Thank you for registering with us!</p>
            <p>Your account has been created successfully.</p>
            <p>Please verify your email to access all features.</p>
        </div>
    </div>
</body>
</html>";
    }
}