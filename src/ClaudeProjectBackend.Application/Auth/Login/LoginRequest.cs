using System.ComponentModel.DataAnnotations;

namespace ClaudeProjectBackend.Application.Auth.Login;

public sealed record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password);
