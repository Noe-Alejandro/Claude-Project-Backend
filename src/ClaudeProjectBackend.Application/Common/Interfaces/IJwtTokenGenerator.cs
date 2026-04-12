using ClaudeProjectBackend.Domain.Entities;

namespace ClaudeProjectBackend.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresIn) GenerateToken(User user);
}
