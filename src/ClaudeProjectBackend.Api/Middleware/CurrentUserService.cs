using System.Security.Claims;
using ClaudeProjectBackend.Application.Common.Interfaces;
using ClaudeProjectBackend.Domain.Enums;

namespace ClaudeProjectBackend.Api.Middleware;

public sealed class CurrentUserService : ICurrentUserService
{
    public long UserId { get; }
    public string Email { get; }
    public UserRole Role { get; }
    public bool IsAdmin => Role == UserRole.Admin;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        var claims = accessor.HttpContext?.User;

        UserId = long.TryParse(
            claims?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0L;

        Email = claims?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        Role = Enum.TryParse<UserRole>(
            claims?.FindFirstValue(ClaimTypes.Role), out var role) ? role : UserRole.Viewer;
    }
}
