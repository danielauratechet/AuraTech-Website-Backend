using AuratechBackend.Constants;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Helpers;
using AuratechBackend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuratechBackend.Controllers;

[Route(ApiRoutes.Prefix + "/auth")]
public sealed class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CustomerRegisterRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.RegisterCustomerAsync(request, cancellationToken));

    [HttpPost("set-password")]
    [AllowAnonymous]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.SetPasswordAsync(request, cancellationToken));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.LoginAsync(request, cancellationToken));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
        => FromResult(await _authService.GetMeAsync(User.GetUserId(), cancellationToken));

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.ForgotPasswordAsync(request, cancellationToken));

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.ChangePasswordAsync(User.GetUserId(), request, cancellationToken));
}
