using AutoMapper;
using FluentAssertions;
using Moq;
using RydePlannr.Application.DTOs.User;
using RydePlannr.Application.Services.Implementations;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IGenericRepository<User>> _userGenericRepositoryMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userGenericRepositoryMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(_userGenericRepositoryMock.Object);

        _service = new UserService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        var user = new User { Id = 1, Username = "rider" };
        var expected = new UserResponseDto { Id = 1, Username = "rider", Role = "User" };

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(1, default)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserResponseDto>(user)).Returns(expected);

        var result = await _service.GetByIdAsync(1);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(99, default)).ReturnsAsync((User?)null);

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPublicByIdAsync_WhenExists_ReturnsDto()
    {
        var user = new User { Id = 1, Username = "rider" };
        var expected = new PublicUserResponseDto { Id = 1, Username = "rider", Role = "User" };

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(1, default)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<PublicUserResponseDto>(user)).Returns(expected);

        var result = await _service.GetPublicByIdAsync(1);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetPublicByIdAsync_WhenNotExists_ReturnsNull()
    {
        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(99, default)).ReturnsAsync((User?)null);

        var result = await _service.GetPublicByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedUsers()
    {
        var users = new List<User> { new() { Id = 1, Username = "rider" } };
        var expected = new List<UserResponseDto> { new() { Id = 1, Username = "rider" } };

        _userRepositoryMock.Setup(r => r.GetAllWithRoleAsync(default)).ReturnsAsync(users);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<UserResponseDto>>(users)).Returns(expected);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotExists_ThrowsKeyNotFoundException()
    {
        _userGenericRepositoryMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((User?)null);

        var act = async () => await _service.DeleteAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesUser()
    {
        var user = new User { Id = 1 };
        _userGenericRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.DeleteAsync(1);

        _userGenericRepositoryMock.Verify(r => r.DeleteAsync(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
