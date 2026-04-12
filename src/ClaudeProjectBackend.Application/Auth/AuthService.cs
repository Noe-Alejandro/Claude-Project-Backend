using ClaudeProjectBackend.Application.Auth.Login;
using ClaudeProjectBackend.Application.Auth.Register;
using ClaudeProjectBackend.Application.Common.Exceptions;
using ClaudeProjectBackend.Application.Common.Interfaces;
using ClaudeProjectBackend.Application.Users;
using ClaudeProjectBackend.Domain.Entities;
using ClaudeProjectBackend.Domain.Enums;
using ClaudeProjectBackend.Domain.Repositories;

namespace ClaudeProjectBackend.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    private readonly ISnowflakeIdGenerator _snowflake;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        ISnowflakeIdGenerator snowflake)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _snowflake = snowflake;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userRepository.SaveChangesAsync(ct);

        var (token, expiresIn) = _jwtTokenGenerator.GenerateToken(user);
        return new LoginResponse(token, expiresIn, UserResponse.FromEntity(user));
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            Id = _snowflake.NewId(),
            Email = request.Email.ToLowerInvariant(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.User,
        };

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        var (token, expiresIn) = _jwtTokenGenerator.GenerateToken(user);
        return new LoginResponse(token, expiresIn, UserResponse.FromEntity(user));
    }
}
