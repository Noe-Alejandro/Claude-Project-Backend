using System.ComponentModel.DataAnnotations;

namespace ClaudeProjectBackend.Application.Auth.Register;

public sealed record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    [Required][MaxLength(100)] string FirstName,
    [Required][MaxLength(100)] string LastName);
