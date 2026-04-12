using ClaudeProjectBackend.Domain.Entities;
using ClaudeProjectBackend.Domain.Enums;

namespace ClaudeProjectBackend.Application.Users.List;

public sealed record UserSummary(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    DateTimeOffset CreatedAt)
{
    public static UserSummary FromEntity(User user) => new(
        user.Id,
        user.Email,
        $"{user.FirstName} {user.LastName}".Trim(),
        user.Role,
        user.CreatedAt);
}
