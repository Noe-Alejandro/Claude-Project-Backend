using ClaudeProjectBackend.Domain.Enums;

namespace ClaudeProjectBackend.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long UserId { get; }
    string Email { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
}
