using System;

namespace EcommerceApp.Application.Services;

public static class EmailLinkBuilder
{
    public static string BuildAbsoluteUrl(string? baseUrl, string relativePath, string? query = null)
    {
        var normalizedBase = string.IsNullOrWhiteSpace(baseUrl)
            ? "http://localhost:5132"
            : baseUrl.Trim();

        if (!normalizedBase.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !normalizedBase.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            normalizedBase = $"https://{normalizedBase}";
        }

        var baseUri = new Uri(normalizedBase, UriKind.Absolute);
        var path = relativePath.StartsWith("/", StringComparison.Ordinal) ? relativePath : $"/{relativePath}";
        var combinedPath = string.IsNullOrWhiteSpace(baseUri.AbsolutePath) || baseUri.AbsolutePath == "/"
            ? path
            : $"{baseUri.AbsolutePath.TrimEnd('/')}{path}";

        var builder = new UriBuilder(baseUri)
        {
            Path = combinedPath,
            Query = query ?? string.Empty
        };

        return builder.Uri.ToString().TrimEnd('/');
    }

    public static string BuildVerificationLink(string? baseUrl, string email, string token)
    {
        var query = $"email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        return BuildAbsoluteUrl(baseUrl, "/api/auth/verify-email", query);
    }

    public static string BuildPasswordResetLink(string? baseUrl, string email, string token)
    {
        var query = $"email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        return BuildAbsoluteUrl(baseUrl, "/reset-password", query);
    }
}
