using ClaudeProjectBackend.Domain.Enums;

namespace ClaudeProjectBackend.Application.Users.Create;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRole Role = UserRole.User);
