namespace EcommerceApp.Application.DTOs.Auth
{
    public static class AuthErrorCodes
    {
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
        public const string AccountDeactivated = "ACCOUNT_DEACTIVATED";
        public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
        public const string InvalidToken = "INVALID_TOKEN";
        public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
        public const string InvalidResetToken = "INVALID_RESET_TOKEN";
        public const string TooManyAttempts = "TOO_MANY_ATTEMPTS";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string UserNotFound = "USER_NOT_FOUND";
    }
}