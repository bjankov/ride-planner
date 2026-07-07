using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using RydePlannr.AuthService.DTOs;
using RydePlannr.AuthService.Services.Implementations;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IGenericRepository<Role>> _roleRepositoryMock;
    private readonly Mock<IGenericRepository<RefreshToken>> _refreshTokenGenericRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthenticationService _service;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IGenericRepository<Role>>();
        _refreshTokenGenericRepositoryMock = new Mock<IGenericRepository<RefreshToken>>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Role>()).Returns(_roleRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<RefreshToken>()).Returns(_refreshTokenGenericRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _configurationMock.Setup(c => c["Jwt:Key"])
            .Returns("toByh5+b5IwzoKZypFSd8FsKvV983oazDfmEwNHU7hJ95tF89lb+juPZM19soKRXrDJo3H+A7CvQAk4Byjsafg==");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("RydePlannr");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("RydePlannr");
        _configurationMock.Setup(c => c.GetSection("Jwt:AccessTokenExpiryMinutes")).Returns(ConfigSection("30"));
        _configurationMock.Setup(c => c.GetSection("Jwt:RefreshTokenExpiryDays")).Returns(ConfigSection("30"));

        _service = new AuthenticationService(_unitOfWorkMock.Object, _configurationMock.Object);
    }

    private static IConfigurationSection ConfigSection(string value)
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s.Value).Returns(value);
        return section.Object;
    }

    private static RegisterDto ValidRegisterDto() => new()
    {
        Username = "testrider",
        Email = "testrider@example.com",
        Password = "Passw0rd!"
    };

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        var dto = ValidRegisterDto();
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, default)).ReturnsAsync(true);

        var act = async () => await _service.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Korisnik s tim emailom već postoji.");
    }

    [Fact]
    public async Task RegisterAsync_WhenDefaultRoleNotFound_ThrowsInvalidOperationException()
    {
        var dto = ValidRegisterDto();
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, default)).ReturnsAsync(false);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(RoleIds.DefaultUser, default)).ReturnsAsync((Role?)null);

        var act = async () => await _service.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Defaultna rola nije pronađena.");
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_HashesPasswordAndReturnsTokenPair()
    {
        var dto = ValidRegisterDto();
        var role = new Role { Id = RoleIds.DefaultUser, Name = "User" };

        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, default)).ReturnsAsync(false);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(RoleIds.DefaultUser, default)).ReturnsAsync(role);

        User? capturedUser = null;
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        RefreshToken? capturedRefreshToken = null;
        _refreshTokenGenericRepositoryMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), default))
            .Callback<RefreshToken, CancellationToken>((rt, _) => capturedRefreshToken = rt)
            .Returns(Task.CompletedTask);

        var result = await _service.RegisterAsync(dto);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        capturedUser.Should().NotBeNull();
        capturedUser!.Username.Should().Be(dto.Username);
        capturedUser.Email.Should().Be(dto.Email);
        capturedUser.PasswordHash.Should().NotBe(dto.Password);
        BCrypt.Net.BCrypt.Verify(dto.Password, capturedUser.PasswordHash).Should().BeTrue();
        capturedUser.RoleId.Should().Be(role.Id);

        capturedRefreshToken.Should().NotBeNull();
        capturedRefreshToken!.TokenHash.Should().NotBeNullOrWhiteSpace();
        capturedRefreshToken.TokenHash.Should().NotBe(result.RefreshToken);
        capturedRefreshToken.User.Should().BeSameAs(capturedUser);

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenSaveThrowsDuplicateEmail_ThrowsFriendlyEmailMessage()
    {
        var dto = ValidRegisterDto();
        var role = new Role { Id = RoleIds.DefaultUser, Name = "User" };

        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, default)).ReturnsAsync(false);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(RoleIds.DefaultUser, default)).ReturnsAsync(role);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ThrowsAsync(new DuplicateFieldException("Email", "duplicate"));

        var act = async () => await _service.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Korisnik s tim emailom već postoji.");
    }

    [Fact]
    public async Task RegisterAsync_WhenSaveThrowsDuplicateUsername_ThrowsFriendlyUsernameMessage()
    {
        var dto = ValidRegisterDto();
        var role = new Role { Id = RoleIds.DefaultUser, Name = "User" };

        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, default)).ReturnsAsync(false);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(RoleIds.DefaultUser, default)).ReturnsAsync(role);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ThrowsAsync(new DuplicateFieldException("Username", "duplicate"));

        var act = async () => await _service.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Korisničko ime je već zauzeto.");
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto { Email = "missing@example.com", Password = "whatever" };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email, default)).ReturnsAsync((User?)null);

        var act = async () => await _service.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Pogrešan email ili lozinka.");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIncorrect_ThrowsUnauthorizedAccessException()
    {
        var user = new User
        {
            Id = 1,
            Email = "testrider@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!"),
            RoleId = RoleIds.DefaultUser
        };
        var dto = new LoginDto { Email = user.Email, Password = "WrongPassword1!" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email, default)).ReturnsAsync(user);

        var act = async () => await _service.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Pogrešan email ili lozinka.");
    }

    [Fact]
    public async Task LoginAsync_WhenRoleNotFound_ThrowsInvalidOperationException()
    {
        var user = new User
        {
            Id = 1,
            Email = "testrider@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!"),
            RoleId = RoleIds.DefaultUser
        };
        var dto = new LoginDto { Email = user.Email, Password = "CorrectPassword1!" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email, default)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(user.RoleId, default)).ReturnsAsync((Role?)null);

        var act = async () => await _service.LoginAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Rola korisnika nije pronađena.");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenPair()
    {
        var user = new User
        {
            Id = 1,
            Username = "testrider",
            Email = "testrider@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!"),
            RoleId = RoleIds.DefaultUser
        };
        var role = new Role { Id = RoleIds.DefaultUser, Name = "User" };
        var dto = new LoginDto { Email = user.Email, Password = "CorrectPassword1!" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email, default)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(user.RoleId, default)).ReturnsAsync(role);

        var result = await _service.LoginAsync(dto);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenNotFound_ThrowsUnauthorizedAccessException()
    {
        var dto = new RefreshRequestDto { RefreshToken = "unknown-token" };
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), default))
            .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _service.RefreshAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Neispravan ili istekao refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenExpired_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Id = 1, Username = "rider", RoleId = RoleIds.DefaultUser, Role = new Role { Name = "User" } };
        var expiredToken = new RefreshToken
        {
            TokenHash = "hash",
            User = user,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        var dto = new RefreshRequestDto { RefreshToken = "some-token" };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), default))
            .ReturnsAsync(expiredToken);

        var act = async () => await _service.RefreshAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Neispravan ili istekao refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenAlreadyRevoked_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Id = 1, Username = "rider", RoleId = RoleIds.DefaultUser, Role = new Role { Name = "User" } };
        var revokedToken = new RefreshToken
        {
            TokenHash = "hash",
            User = user,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var dto = new RefreshRequestDto { RefreshToken = "some-token" };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), default))
            .ReturnsAsync(revokedToken);

        var act = async () => await _service.RefreshAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Neispravan ili istekao refresh token.");
    }

    [Fact]
    public async Task RefreshAsync_WithValidToken_RotatesAndReturnsNewTokenPair()
    {
        var user = new User { Id = 1, Username = "rider", RoleId = RoleIds.DefaultUser, Role = new Role { Name = "User" } };
        var activeToken = new RefreshToken
        {
            TokenHash = "old-hash",
            User = user,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        var dto = new RefreshRequestDto { RefreshToken = "valid-token" };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), default))
            .ReturnsAsync(activeToken);

        RefreshToken? capturedNewToken = null;
        _refreshTokenGenericRepositoryMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), default))
            .Callback<RefreshToken, CancellationToken>((rt, _) => capturedNewToken = rt)
            .Returns(Task.CompletedTask);

        var result = await _service.RefreshAsync(dto);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();

        activeToken.RevokedAt.Should().NotBeNull();
        capturedNewToken.Should().NotBeNull();
        capturedNewToken!.TokenHash.Should().NotBe(activeToken.TokenHash);

        _refreshTokenGenericRepositoryMock.Verify(r => r.UpdateAsync(activeToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RevokeAsync_WhenTokenNotFound_DoesNothing()
    {
        var dto = new RefreshRequestDto { RefreshToken = "unknown-token" };
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), default))
            .ReturnsAsync((RefreshToken?)null);

        await _service.RevokeAsync(dto);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task RevokeAsync_WithActiveToken_RevokesIt()
    {
        var user = new User { Id = 1, Username = "rider", RoleId = RoleIds.DefaultUser, Role = new Role { Name = "User" } };
        var activeToken = new RefreshToken
        {
            TokenHash = "hash",
            User = user,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        var dto = new RefreshRequestDto { RefreshToken = "valid-token" };

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), default))
            .ReturnsAsync(activeToken);

        await _service.RevokeAsync(dto);

        activeToken.RevokedAt.Should().NotBeNull();
        _refreshTokenGenericRepositoryMock.Verify(r => r.UpdateAsync(activeToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
