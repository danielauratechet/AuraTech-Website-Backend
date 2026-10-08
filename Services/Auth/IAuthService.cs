using AuratechBackend.Core.Results;
using AuratechBackend.Data.Dtos.Requests;
using AuratechBackend.Data.Dtos.Responses;

namespace AuratechBackend.Services.Auth;

public interface IAuthService
{
    Task<Result> RegisterCustomerAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<MeResponse>> GetMeAsync(int userId, CancellationToken cancellationToken = default);

    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
