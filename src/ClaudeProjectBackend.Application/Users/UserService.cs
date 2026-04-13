using ClaudeProjectBackend.Application.Common;
using ClaudeProjectBackend.Application.Common.Exceptions;
using ClaudeProjectBackend.Application.Common.Interfaces;
using ClaudeProjectBackend.Application.Users.Create;
using ClaudeProjectBackend.Application.Users.List;
using ClaudeProjectBackend.Application.Users.UpdateAvatar;
using ClaudeProjectBackend.Domain.Entities;
using ClaudeProjectBackend.Domain.Repositories;

namespace ClaudeProjectBackend.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISnowflakeIdGenerator _snowflake;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISnowflakeIdGenerator snowflake)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _snowflake = snowflake;
    }

    public async Task<UserResponse> GetAsync(long id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);

        return UserResponse.FromEntity(user);
    }

    public async Task<PagedResponse<UserSummary>> ListAsync(ListUsersQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _userRepository.ListAsync(query.Page, query.PageSize, ct);

        return new PagedResponse<UserSummary>(
            items.Select(UserSummary.FromEntity),
            total,
            query.Page,
            query.PageSize);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new ConflictException($"Email '{request.Email}' is already in use.");

        var user = new User
        {
            Id = _snowflake.NewId(),
            Email = request.Email.ToLowerInvariant(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
        };

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        return UserResponse.FromEntity(user);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);

        await _userRepository.DeleteAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
    }

    public async Task<UserResponse> UpdateAvatarAsync(long id, UpdateAvatarRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);

        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

        await _userRepository.SaveChangesAsync(ct);

        return UserResponse.FromEntity(user);
    }
}
