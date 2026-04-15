using System.ComponentModel.DataAnnotations;
using ClaudeProjectBackend.Domain.Enums;

namespace ClaudeProjectBackend.Application.Users.Create;

public sealed record CreateUserRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    [Required][MaxLength(100)] string FirstName,
    [Required][MaxLength(100)] string LastName,
    UserRole Role = UserRole.User);
