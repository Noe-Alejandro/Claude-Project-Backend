namespace ClaudeProjectBackend.Tests.Auth;

public sealed class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ISnowflakeIdGenerator _snowflake = Substitute.For<ISnowflakeIdGenerator>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _snowflake.NewId().Returns(375296004000000099L);
        _sut = new AuthService(_userRepository, _jwtTokenGenerator, _passwordHasher, _snowflake);
    }

    // ── LoginAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenAndUser()
    {
        var user = new User
        {
            Id = 375296004000000001L,
            Email = "test@example.com",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User
        };

        _userRepository.GetByEmailAsync("test@example.com").Returns(user);
        _passwordHasher.Verify("password123", user.PasswordHash).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns(("tok123", 3600));

        var result = await _sut.LoginAsync(new LoginRequest("test@example.com", "password123"));

        result.AccessToken.Should().Be("tok123");
        result.ExpiresIn.Should().Be(3600);
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        await _sut.Invoking(s => s.LoginAsync(new LoginRequest("nobody@example.com", "pass")))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = new User { Email = "test@example.com" };
        _userRepository.GetByEmailAsync("test@example.com").Returns(user);
        _passwordHasher.Verify("wrong", user.PasswordHash).Returns(false);

        await _sut.Invoking(s => s.LoginAsync(new LoginRequest("test@example.com", "wrong")))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsToken()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_pw");
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>()).Returns(("new_tok", 3600));

        var result = await _sut.RegisterAsync(
            new RegisterRequest("new@example.com", "password123", "John", "Doe"));

        result.AccessToken.Should().Be("new_tok");
        result.User.Email.Should().Be("new@example.com");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "new@example.com" && u.Role == UserRole.User));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflictException()
    {
        var existing = new User { Email = "taken@example.com" };
        _userRepository.GetByEmailAsync("taken@example.com").Returns(existing);

        await _sut.Invoking(s => s.RegisterAsync(
                new RegisterRequest("taken@example.com", "pass", "A", "B")))
            .Should().ThrowAsync<ConflictException>();

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }
}
