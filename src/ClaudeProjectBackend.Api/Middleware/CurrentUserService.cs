using System.Security.Claims;
using ClaudeProjectBackend.Application.Common.Interfaces;
using ClaudeProjectBackend.Domain.Enums;

namespace ClaudeProjectBackend.Api.Middleware;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid UserId { get; }
    public string Email { get; }
    public UserRole Role { get; }
    public bool IsAdmin => Role == UserRole.Admin;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        var claims = accessor.HttpContext?.User;

        UserId = Guid.TryParse(
            claims?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        Email = claims?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        Role = Enum.TryParse<UserRole>(
            claims?.FindFirstValue(ClaimTypes.Role), out var role) ? role : UserRole.Viewer;
    }
}
