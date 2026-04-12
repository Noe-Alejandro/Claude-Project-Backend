using ClaudeProjectBackend.Application.Common;
using ClaudeProjectBackend.Application.Users.Create;
using ClaudeProjectBackend.Application.Users.List;

namespace ClaudeProjectBackend.Application.Users;

public interface IUserService
{
    Task<UserResponse> GetAsync(long id, CancellationToken ct = default);
    Task<PagedResponse<UserSummary>> ListAsync(ListUsersQuery query, CancellationToken ct = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
