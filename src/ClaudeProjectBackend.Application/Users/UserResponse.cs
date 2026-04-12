using ClaudeProjectBackend.Domain.Entities;
using ClaudeProjectBackend.Domain.Enums;

namespace ClaudeProjectBackend.Application.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    string? AvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt)
{
    public static UserResponse FromEntity(User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        $"{user.FirstName} {user.LastName}".Trim(),
        user.Role,
        user.AvatarUrl,
        user.CreatedAt,
        user.LastLoginAt);
}
