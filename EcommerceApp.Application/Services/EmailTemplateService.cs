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
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background-color: #4CAF50; padding: 20px; text-align: center;'>
            <h1 style='color: white; margin: 0;'>Welcome to EcommerceApp!</h1>
        </div>
        
        <div style='padding: 20px; background-color: #f9f9f9;'>
            <h2>Hello {fullName},</h2>
            <p>Thank you for registering with us. Please verify your email address to activate your account.</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{verificationLink}' 
                   style='background-color: #4CAF50; color: white; padding: 12px 24px; 
                          text-decoration: none; border-radius: 4px; display: inline-block;'>
                    Verify Email Address
                </a>
            </div>
            
            <p>Or copy this link to your browser:</p>
            <p style='background-color: #eee; padding: 10px; word-break: break-all;'>{verificationLink}</p>
            
            <p><strong>This link will expire in 24 hours.</strong></p>
            
            <hr style='margin: 20px 0;'>
            
            <p style='color: #666; font-size: 12px;'>
                If you didn't create an account with EcommerceApp, please ignore this email.
            </p>
        </div>
        
        <div style='text-align: center; padding: 20px; font-size: 12px; color: #666;'>
            <p>&copy; 2024 EcommerceApp. All rights reserved.</p>
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
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background-color: #ff9800; padding: 20px; text-align: center;'>
            <h1 style='color: white; margin: 0;'>Password Reset Request</h1>
        </div>
        
        <div style='padding: 20px; background-color: #f9f9f9;'>
            <h2>Hello {fullName},</h2>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}' 
                   style='background-color: #ff9800; color: white; padding: 12px 24px; 
                          text-decoration: none; border-radius: 4px; display: inline-block;'>
                    Reset Password
                </a>
            </div>
            
            <p>Or copy this link to your browser:</p>
            <p style='background-color: #eee; padding: 10px; word-break: break-all;'>{resetLink}</p>
            
            <p><strong>This link will expire in 1 hour.</strong></p>
            
            <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0;'>
                <p style='margin: 0; color: #856404;'>
                    ⚠️ If you did not request a password reset, please ignore this email and your password will remain unchanged.
                </p>
            </div>
        </div>
        
        <div style='text-align: center; padding: 20px; font-size: 12px; color: #666;'>
            <p>&copy; 2024 EcommerceApp. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}