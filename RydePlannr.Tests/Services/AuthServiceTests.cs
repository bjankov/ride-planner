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
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthenticationService _service;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IGenericRepository<Role>>();
        _configurationMock = new Mock<IConfiguration>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Role>()).Returns(_roleRepositoryMock.Object);

        _configurationMock.Setup(c => c["Jwt:Key"])
            .Returns("toByh5+b5IwzoKZypFSd8FsKvV983oazDfmEwNHU7hJ95tF89lb+juPZM19soKRXrDJo3H+A7CvQAk4Byjsafg==");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("RydePlannr");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("RydePlannr");

        _service = new AuthenticationService(_unitOfWorkMock.Object, _configurationMock.Object);
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
    public async Task RegisterAsync_WithValidData_HashesPasswordAndReturnsToken()
    {
        var dto = ValidRegisterDto();
        var role = new Role { Id = RoleIds.DefaultUser, Name = "User" };

        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, default)).ReturnsAsync(false);
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(RoleIds.DefaultUser, default)).ReturnsAsync(role);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        User? capturedUser = null;
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        var token = await _service.RegisterAsync(dto);

        token.Should().NotBeNullOrWhiteSpace();
        capturedUser.Should().NotBeNull();
        capturedUser!.Username.Should().Be(dto.Username);
        capturedUser.Email.Should().Be(dto.Email);
        capturedUser.PasswordHash.Should().NotBe(dto.Password);
        BCrypt.Net.BCrypt.Verify(dto.Password, capturedUser.PasswordHash).Should().BeTrue();
        capturedUser.RoleId.Should().Be(role.Id);

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
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
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

        var token = await _service.LoginAsync(dto);

        token.Should().NotBeNullOrWhiteSpace();
    }
}
