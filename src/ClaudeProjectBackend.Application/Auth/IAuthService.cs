using ClaudeProjectBackend.Application.Auth.Login;
using ClaudeProjectBackend.Application.Auth.Register;

namespace ClaudeProjectBackend.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}
