using ClaudeProjectBackend.Application.Common;
using ClaudeProjectBackend.Application.Common.Exceptions;
using ClaudeProjectBackend.Application.Common.Interfaces;
using ClaudeProjectBackend.Application.Users.Create;
using ClaudeProjectBackend.Application.Users.List;
using ClaudeProjectBackend.Domain.Entities;
using ClaudeProjectBackend.Domain.Repositories;

namespace ClaudeProjectBackend.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> GetAsync(Guid id, CancellationToken ct = default)
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
            Id = Guid.NewGuid(),
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

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);

        await _userRepository.DeleteAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
    }
}
