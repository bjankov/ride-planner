using AutoMapper;
using FluentAssertions;
using Moq;
using RydePlannr.Application.DTOs.Club;
using RydePlannr.Application.Services.Implementations;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Tests.Services;

public class ClubServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IClubRepository> _clubRepositoryMock;
    private readonly Mock<IGenericRepository<Club>> _clubGenericRepositoryMock;
    private readonly Mock<IClubMemberRepository> _clubMemberRepositoryMock;
    private readonly Mock<IGenericRepository<ClubMember>> _clubMemberGenericRepositoryMock;
    private readonly ClubService _service;

    public ClubServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _clubRepositoryMock = new Mock<IClubRepository>();
        _clubGenericRepositoryMock = new Mock<IGenericRepository<Club>>();
        _clubMemberRepositoryMock = new Mock<IClubMemberRepository>();
        _clubMemberGenericRepositoryMock = new Mock<IGenericRepository<ClubMember>>();

        _unitOfWorkMock.Setup(u => u.Clubs).Returns(_clubRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ClubMembers).Returns(_clubMemberRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Club>()).Returns(_clubGenericRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<ClubMember>()).Returns(_clubMemberGenericRepositoryMock.Object);

        _service = new ClubService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedClubs()
    {
        var clubs = new List<Club> { new() { Id = 1, Name = "Club A" } };
        var expected = new List<ClubResponseDto> { new() { Id = 1, Name = "Club A" } };

        _clubRepositoryMock.Setup(r => r.GetAllWithMembersAsync(default)).ReturnsAsync(clubs);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<ClubResponseDto>>(clubs)).Returns(expected);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WhenClubExists_ReturnsDto()
    {
        var club = new Club { Id = 1, Name = "Club A" };
        var expected = new ClubResponseDto { Id = 1, Name = "Club A" };

        _clubRepositoryMock.Setup(r => r.GetByIdWithMembersAsync(1, default)).ReturnsAsync(club);
        _mapperMock.Setup(m => m.Map<ClubResponseDto>(club)).Returns(expected);

        var result = await _service.GetByIdAsync(1);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WhenClubDoesNotExist_ReturnsNull()
    {
        _clubRepositoryMock.Setup(r => r.GetByIdWithMembersAsync(99, default)).ReturnsAsync((Club?)null);

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAtAndPersistsClub()
    {
        var dto = new CreateClubDto { Name = "New Club", Description = "desc" };
        var mappedClub = new Club { Name = dto.Name, Description = dto.Description };
        var expected = new ClubResponseDto { Name = dto.Name };

        _mapperMock.Setup(m => m.Map<Club>(dto)).Returns(mappedClub);
        _mapperMock.Setup(m => m.Map<ClubResponseDto>(mappedClub)).Returns(expected);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.CreateAsync(dto);

        result.Should().Be(expected);
        mappedClub.CreatedAt.Should().NotBe(default);
        _clubGenericRepositoryMock.Verify(r => r.AddAsync(mappedClub, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetMembersAsync_WhenClubDoesNotExist_ThrowsKeyNotFoundException()
    {
        _clubGenericRepositoryMock.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        var act = async () => await _service.GetMembersAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetMembersAsync_WhenClubExists_ReturnsMappedMembers()
    {
        var members = new List<ClubMember> { new() { UserId = 1, ClubId = 1 } };
        var expected = new List<ClubMemberResponseDto> { new() { UserId = 1, Username = "rider" } };

        _clubGenericRepositoryMock.Setup(r => r.ExistsAsync(1, default)).ReturnsAsync(true);
        _clubMemberRepositoryMock.Setup(r => r.GetByClubAsync(1, default)).ReturnsAsync(members);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<ClubMemberResponseDto>>(members)).Returns(expected);

        var result = await _service.GetMembersAsync(1);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task JoinAsync_WhenClubDoesNotExist_ThrowsKeyNotFoundException()
    {
        _clubGenericRepositoryMock.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        var act = async () => await _service.JoinAsync(99, userId: 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task JoinAsync_WhenAlreadyMemberOfSameClub_ThrowsInvalidOperationException()
    {
        _clubGenericRepositoryMock.Setup(r => r.ExistsAsync(1, default)).ReturnsAsync(true);
        _clubMemberRepositoryMock.Setup(r => r.GetActiveMembershipForUserAsync(1, default))
            .ReturnsAsync(new ClubMember { ClubId = 1, UserId = 1 });

        var act = async () => await _service.JoinAsync(1, userId: 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Već ste član ovog kluba.");
    }

    [Fact]
    public async Task JoinAsync_WhenMemberOfDifferentClub_ThrowsInvalidOperationException()
    {
        _clubGenericRepositoryMock.Setup(r => r.ExistsAsync(2, default)).ReturnsAsync(true);
        _clubMemberRepositoryMock.Setup(r => r.GetActiveMembershipForUserAsync(1, default))
            .ReturnsAsync(new ClubMember { ClubId = 1, UserId = 1 });

        var act = async () => await _service.JoinAsync(2, userId: 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Već ste član drugog kluba. Napustite ga prije pristupanja novom.");
    }

    [Fact]
    public async Task JoinAsync_WithValidData_PersistsMembership()
    {
        _clubGenericRepositoryMock.Setup(r => r.ExistsAsync(1, default)).ReturnsAsync(true);
        _clubMemberRepositoryMock.Setup(r => r.GetActiveMembershipForUserAsync(1, default))
            .ReturnsAsync((ClubMember?)null);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.JoinAsync(1, userId: 1);

        _clubMemberGenericRepositoryMock.Verify(
            r => r.AddAsync(It.Is<ClubMember>(m => m.ClubId == 1 && m.UserId == 1), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task JoinAsync_WhenSaveThrowsDuplicateField_ThrowsAlreadyMemberMessage()
    {
        _clubGenericRepositoryMock.Setup(r => r.ExistsAsync(1, default)).ReturnsAsync(true);
        _clubMemberRepositoryMock.Setup(r => r.GetActiveMembershipForUserAsync(1, default))
            .ReturnsAsync((ClubMember?)null);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ThrowsAsync(new DuplicateFieldException("Membership", "duplicate"));

        var act = async () => await _service.JoinAsync(1, userId: 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Već ste član ovog kluba.");
    }

    [Fact]
    public async Task LeaveAsync_WhenNotMember_ThrowsInvalidOperationException()
    {
        _clubMemberRepositoryMock.Setup(r => r.GetMembershipAsync(1, 1, default))
            .ReturnsAsync((ClubMember?)null);

        var act = async () => await _service.LeaveAsync(1, userId: 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Niste član ovog kluba.");
    }

    [Fact]
    public async Task LeaveAsync_WhenMember_RemovesMembership()
    {
        var membership = new ClubMember { ClubId = 1, UserId = 1 };
        _clubMemberRepositoryMock.Setup(r => r.GetMembershipAsync(1, 1, default)).ReturnsAsync(membership);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.LeaveAsync(1, userId: 1);

        _clubMemberGenericRepositoryMock.Verify(r => r.DeleteAsync(membership), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenClubDoesNotExist_ThrowsKeyNotFoundException()
    {
        _clubGenericRepositoryMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Club?)null);

        var act = async () => await _service.DeleteAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenClubExists_RemovesClub()
    {
        var club = new Club { Id = 1 };
        _clubGenericRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(club);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.DeleteAsync(1);

        _clubGenericRepositoryMock.Verify(r => r.DeleteAsync(club), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
