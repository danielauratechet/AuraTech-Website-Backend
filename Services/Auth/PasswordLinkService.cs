using System.Text;
using AuratechBackend.Constants;
using AuratechBackend.Core.Results;
using AuratechBackend.Data.Entities;
using AuratechBackend.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AuratechBackend.Services.Auth;

public interface IPasswordLinkService
{
    Task<Result> SendSetPasswordLinkAsync(ApplicationUser user, string subject, CancellationToken cancellationToken = default);
}

public sealed class PasswordLinkService : IPasswordLinkService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasswordLinkService> _logger;

    public PasswordLinkService(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<PasswordLinkService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result> SendSetPasswordLinkAsync(
        ApplicationUser user,
        string subject,
        CancellationToken cancellationToken = default)
    {
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var opaqueToken = CreateOpaqueToken(user.Id, resetToken);
        var baseUrl = (_configuration["App:PublicBaseUrl"] ?? "http://localhost:5048").TrimEnd('/');
        var link = $"{baseUrl}/set-password?token={Uri.EscapeDataString(opaqueToken)}";

        var body = $"""
            <p>Hello,</p>
            <p>Use the link below to set your password. This link expires in 24 hours.</p>
            <p><a href="{link}">{link}</a></p>
            """;

        try
        {
            await _emailSender.SendAsync(user.Email!, subject, body, cancellationToken);
            return Result.Success("Password link email sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password link email to {Email}", user.Email);
            return Result.Failure(
                ErrorCodes.Failure,
                "Unable to send email. Please try again later.");
        }
    }

    public static string CreateOpaqueToken(int userId, string identityResetToken)
    {
        var encodedResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(identityResetToken));
        var payload = $"{userId}.{encodedResetToken}";
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
    }

    public static bool TryParseOpaqueToken(string opaqueToken, out int userId, out string identityResetToken)
    {
        userId = 0;
        identityResetToken = string.Empty;

        try
        {
            var payload = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(opaqueToken));
            var separatorIndex = payload.IndexOf('.');
            if (separatorIndex <= 0 || separatorIndex == payload.Length - 1)
            {
                return false;
            }

            if (!int.TryParse(payload[..separatorIndex], out userId) || userId <= 0)
            {
                return false;
            }

            var encodedResetToken = payload[(separatorIndex + 1)..];
            identityResetToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedResetToken));
            return !string.IsNullOrWhiteSpace(identityResetToken);
        }
        catch
        {
            return false;
        }
    }
}
