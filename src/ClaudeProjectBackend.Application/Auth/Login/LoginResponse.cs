using ClaudeProjectBackend.Application.Users;

namespace ClaudeProjectBackend.Application.Auth.Login;

public sealed record LoginResponse(string AccessToken, int ExpiresIn, UserResponse User);
