namespace ClaudeProjectBackend.Tests.Users;

public sealed class UserServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ISnowflakeIdGenerator _snowflake = Substitute.For<ISnowflakeIdGenerator>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _snowflake.NewId().Returns(375296004000000001L);
        // Default: act as the owner of id 375296004000000001, non-admin
        _currentUser.UserId.Returns(375296004000000001L);
        _currentUser.IsAdmin.Returns(false);
        _sut = new UserService(_userRepository, _passwordHasher, _snowflake, _currentUser);
    }

    // ── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ExistingId_ReturnsUserResponse()
    {
        const long id = 375296004000000001L;
        var user = new User { Id = id, Email = "u@example.com", FirstName = "A", LastName = "B" };
        _userRepository.GetByIdAsync(id).Returns(user);

        var result = await _sut.GetAsync(id);

        result.Id.Should().Be(id.ToString());
        result.Email.Should().Be("u@example.com");
        result.FullName.Should().Be("A B");
    }

    [Fact]
    public async Task GetAsync_UnknownId_ThrowsNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<long>()).Returns((User?)null);

        await _sut.Invoking(s => s.GetAsync(999L))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ── ListAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_ReturnsPagedResponse()
    {
        var users = new List<User>
        {
            new() { Id = 375296004000000001L, Email = "a@example.com", FirstName = "A", LastName = "A" },
            new() { Id = 375296004000000002L, Email = "b@example.com", FirstName = "B", LastName = "B" },
        };

        _userRepository.ListAsync(1, 20).Returns(((IReadOnlyList<User>)users, 2));

        var result = await _sut.ListAsync(new ListUsersQuery(1, 20));

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_NewEmail_CreatesAndReturnsUser()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        _passwordHasher.Hash("secret").Returns("hashed");

        var result = await _sut.CreateAsync(
            new CreateUserRequest("new@example.com", "secret", "Jane", "Smith", UserRole.Manager));

        result.Email.Should().Be("new@example.com");
        result.Role.Should().Be(UserRole.Manager);
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.Role == UserRole.Manager));
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsConflictException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns(new User());

        await _sut.Invoking(s => s.CreateAsync(
                new CreateUserRequest("taken@example.com", "pass", "X", "Y")))
            .Should().ThrowAsync<ConflictException>();
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingUser_Deletes()
    {
        const long id = 375296004000000001L;
        var user = new User { Id = id };
        _userRepository.GetByIdAsync(id).Returns(user);

        await _sut.DeleteAsync(id);

        await _userRepository.Received(1).DeleteAsync(user);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<long>()).Returns((User?)null);

        await _sut.Invoking(s => s.DeleteAsync(999L))
            .Should().ThrowAsync<NotFoundException>();
    }
}
