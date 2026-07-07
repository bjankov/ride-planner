using AutoMapper;
using FluentAssertions;
using Moq;
using RydePlannr.Application.DTOs.RideEvent;
using RydePlannr.Application.Services.Implementations;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Enums;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Tests.Services;

public class RideEventServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly RideEventService _service;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<RideEvent>> _rideEventRepositoryMock;
    private readonly Mock<IGenericRepository<Route>> _routeRepositoryMock;
    private readonly Mock<IGenericRepository<RideType>> _rideTypeRepositoryMock;
    private readonly Mock<IGenericRepository<Club>> _clubRepositoryMock;
    private readonly Mock<IGenericRepository<RideParticipant>> _participantRepositoryMock;

    public RideEventServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _rideEventRepositoryMock = new Mock<IGenericRepository<RideEvent>>();
        _routeRepositoryMock = new Mock<IGenericRepository<Route>>();
        _rideTypeRepositoryMock = new Mock<IGenericRepository<RideType>>();
        _clubRepositoryMock = new Mock<IGenericRepository<Club>>();
        _participantRepositoryMock = new Mock<IGenericRepository<RideParticipant>>();

        _unitOfWorkMock.Setup(u => u.Repository<RideEvent>()).Returns(_rideEventRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Route>()).Returns(_routeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<RideType>()).Returns(_rideTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Club>()).Returns(_clubRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<RideParticipant>()).Returns(_participantRepositoryMock.Object);

        _service = new RideEventService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    private static CreateRideEventDto ValidCreateDto() => new()
    {
        Title = "Test Ride",
        Description = "Test Description",
        StartTime = DateTime.UtcNow.AddDays(1),
        EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
        MaxParticipants = 20,
        RouteId = 1,
        RideTypeId = 1
    };

    private void SetUpValidFks(CreateRideEventDto dto)
    {
        _routeRepositoryMock.Setup(r => r.ExistsAsync(dto.RouteId, default)).ReturnsAsync(true);
        _rideTypeRepositoryMock.Setup(r => r.ExistsAsync(dto.RideTypeId, default)).ReturnsAsync(true);
        if (dto.ClubId is not null)
            _clubRepositoryMock.Setup(r => r.ExistsAsync(dto.ClubId.Value, default)).ReturnsAsync(true);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsRideEvent()
    {
        var dto = ValidCreateDto();
        var expectedResponse = new RideEventResponseDto { Id = 1, Title = "Test Ride", Status = "Planned" };

        SetUpValidFks(dto);

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new RideEvent { Id = 1, Title = dto.Title });

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);

        _mapperMock.Setup(m => m.Map<RideEvent>(dto))
            .Returns(new RideEvent { Title = dto.Title });

        _mapperMock.Setup(m => m.Map<RideEventResponseDto>(It.IsAny<RideEvent>()))
            .Returns(expectedResponse);

        var result = await _service.CreateAsync(dto, 1);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Ride");
        result.Status.Should().Be("Planned");

        _rideEventRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RideEvent>(), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRouteDoesNotExist_ThrowsKeyNotFoundException()
    {
        var dto = ValidCreateDto();
        _routeRepositoryMock.Setup(r => r.ExistsAsync(dto.RouteId, default)).ReturnsAsync(false);

        var act = async () => await _service.CreateAsync(dto, organizerId: 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenRideTypeDoesNotExist_ThrowsKeyNotFoundException()
    {
        var dto = ValidCreateDto();
        _routeRepositoryMock.Setup(r => r.ExistsAsync(dto.RouteId, default)).ReturnsAsync(true);
        _rideTypeRepositoryMock.Setup(r => r.ExistsAsync(dto.RideTypeId, default)).ReturnsAsync(false);

        var act = async () => await _service.CreateAsync(dto, organizerId: 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenClubDoesNotExist_ThrowsKeyNotFoundException()
    {
        var dto = ValidCreateDto();
        dto.ClubId = 7;
        _routeRepositoryMock.Setup(r => r.ExistsAsync(dto.RouteId, default)).ReturnsAsync(true);
        _rideTypeRepositoryMock.Setup(r => r.ExistsAsync(dto.RideTypeId, default)).ReturnsAsync(true);
        _clubRepositoryMock.Setup(r => r.ExistsAsync(dto.ClubId.Value, default)).ReturnsAsync(false);

        var act = async () => await _service.CreateAsync(dto, organizerId: 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenRideExists_ReturnsDto()
    {
        var rideEvent = new RideEvent { Id = 1, Title = "Test Ride" };
        var responseDto = new RideEventResponseDto { Id = 1, Title = "Test Ride" };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default))
            .ReturnsAsync(rideEvent);

        _mapperMock.Setup(m => m.Map<RideEventResponseDto>(rideEvent))
            .Returns(responseDto);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Test Ride");
    }

    [Fact]
    public async Task GetByIdAsync_WhenRideDoesNotExist_ReturnsNull()
    {
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(99, default))
            .ReturnsAsync((RideEvent?)null);

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllUpcomingAsync_ReturnsMappedRides()
    {
        var rides = new List<RideEvent> { new() { Id = 1, Title = "Upcoming" } };
        var expected = new List<RideEventResponseDto> { new() { Id = 1, Title = "Upcoming" } };

        _unitOfWorkMock.Setup(u => u.Rides.GetUpcomingRidesAsync(default)).ReturnsAsync(rides);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<RideEventResponseDto>>(rides)).Returns(expected);

        var result = await _service.GetAllUpcomingAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedRides()
    {
        var rides = new List<RideEvent> { new() { Id = 1, Title = "Any" } };
        var expected = new List<RideEventResponseDto> { new() { Id = 1, Title = "Any" } };

        _unitOfWorkMock.Setup(u => u.Rides.GetAllWithDetailsAsync(default)).ReturnsAsync(rides);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<RideEventResponseDto>>(rides)).Returns(expected);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetByClubAsync_ReturnsMappedRides()
    {
        var rides = new List<RideEvent> { new() { Id = 1, Title = "Club Ride", ClubId = 5 } };
        var expected = new List<RideEventResponseDto> { new() { Id = 1, Title = "Club Ride" } };

        _unitOfWorkMock.Setup(u => u.Rides.GetRidesByClubAsync(5, default)).ReturnsAsync(rides);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<RideEventResponseDto>>(rides)).Returns(expected);

        var result = await _service.GetByClubAsync(5);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateAsync_WhenRideDoesNotExist_ThrowsKeyNotFoundException()
    {
        _rideEventRepositoryMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((RideEvent?)null);

        var act = async () => await _service.UpdateAsync(
            99, new UpdateRideEventDto(), actingUserId: 1, isAdmin: false);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenNotOrganizerAndNotAdmin_ThrowsForbiddenException()
    {
        var rideEvent = new RideEvent { Id = 1, OrganizerId = 42 };
        _rideEventRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(rideEvent);

        var act = async () => await _service.UpdateAsync(
            1, new UpdateRideEventDto(), actingUserId: 1, isAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenOrganizer_UpdatesFieldsAndReturnsReloadedDto()
    {
        var rideEvent = new RideEvent { Id = 1, OrganizerId = 1, Title = "Old Title" };
        var updateDto = new UpdateRideEventDto { Title = "New Title", MaxParticipants = 30 };
        var reloaded = new RideEvent { Id = 1, Title = "New Title" };
        var expected = new RideEventResponseDto { Id = 1, Title = "New Title" };

        _rideEventRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(rideEvent);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(reloaded);
        _mapperMock.Setup(m => m.Map<RideEventResponseDto>(reloaded)).Returns(expected);

        var result = await _service.UpdateAsync(1, updateDto, actingUserId: 1, isAdmin: false);

        result.Should().Be(expected);
        rideEvent.Title.Should().Be("New Title");
        rideEvent.MaxParticipants.Should().Be(30);
        _rideEventRepositoryMock.Verify(r => r.UpdateAsync(rideEvent), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenAdminButNotOrganizer_IsAllowed()
    {
        var rideEvent = new RideEvent { Id = 1, OrganizerId = 42, Title = "Old Title" };
        var updateDto = new UpdateRideEventDto { Title = "Admin Edit" };
        var reloaded = new RideEvent { Id = 1, Title = "Admin Edit" };
        var expected = new RideEventResponseDto { Id = 1, Title = "Admin Edit" };

        _rideEventRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(rideEvent);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(reloaded);
        _mapperMock.Setup(m => m.Map<RideEventResponseDto>(reloaded)).Returns(expected);

        var result = await _service.UpdateAsync(1, updateDto, actingUserId: 1, isAdmin: true);

        result.Should().Be(expected);
        rideEvent.Title.Should().Be("Admin Edit");
    }

    [Fact]
    public async Task DeleteAsync_WhenRideDoesNotExist_ThrowsKeyNotFoundException()
    {
        _rideEventRepositoryMock.Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((RideEvent?)null);

        var act = async () => await _service.DeleteAsync(99, actingUserId: 1, isAdmin: true);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenNotOrganizerAndNotAdmin_ThrowsForbiddenException()
    {
        var rideEvent = new RideEvent { Id = 1, OrganizerId = 42 };
        _rideEventRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(rideEvent);

        var act = async () => await _service.DeleteAsync(1, actingUserId: 1, isAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenOrganizer_RemovesRide()
    {
        var rideEvent = new RideEvent { Id = 1, OrganizerId = 1 };
        _rideEventRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(rideEvent);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.DeleteAsync(1, actingUserId: 1, isAdmin: false);

        _rideEventRepositoryMock.Verify(r => r.DeleteAsync(rideEvent), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task JoinRideAsync_WhenRideDoesNotExist_ThrowsKeyNotFoundException()
    {
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(99, default))
            .ReturnsAsync((RideEvent?)null);

        var act = async () => await _service.JoinRideAsync(99, userId: 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task JoinRideAsync_WhenRideIsFull_ThrowsInvalidOperationException()
    {
        var rideEvent = new RideEvent
        {
            Id = 1,
            MaxParticipants = 1,
            Status = RideStatus.Planned,
            Participants = new List<RideParticipant>
            {
                new() { UserId = 5 }
            }
        };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default))
            .ReturnsAsync(rideEvent);

        var act = async () => await _service.JoinRideAsync(1, userId: 99);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Događaj je popunjen.");
    }

    [Fact]
    public async Task JoinRideAsync_WhenUserAlreadyJoined_ThrowsInvalidOperationException()
    {
        var rideEvent = new RideEvent
        {
            Id = 1,
            MaxParticipants = 10,
            Status = RideStatus.Planned,
            Participants = new List<RideParticipant>
            {
                new() { UserId = 5 }
            }
        };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default))
            .ReturnsAsync(rideEvent);

        var act = async () => await _service.JoinRideAsync(1, userId: 5);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Već ste prijavljeni na ovaj događaj.");
    }

    [Fact]
    public async Task JoinRideAsync_WhenRideNotPlanned_ThrowsInvalidOperationException()
    {
        var rideEvent = new RideEvent
        {
            Id = 1,
            MaxParticipants = 10,
            Status = RideStatus.Cancelled,
            Participants = new List<RideParticipant>()
        };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(rideEvent);

        var act = async () => await _service.JoinRideAsync(1, userId: 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Na ovaj događaj nije se moguće prijaviti.");
    }

    [Fact]
    public async Task JoinRideAsync_WithValidData_PersistsParticipant()
    {
        var rideEvent = new RideEvent
        {
            Id = 1,
            MaxParticipants = 10,
            Status = RideStatus.Planned,
            Participants = new List<RideParticipant>()
        };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(rideEvent);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.JoinRideAsync(1, userId: 7);

        _participantRepositoryMock.Verify(
            r => r.AddAsync(It.Is<RideParticipant>(p => p.RideEventId == 1 && p.UserId == 7), default),
            Times.Once);
    }

    [Fact]
    public async Task JoinRideAsync_WhenSaveThrowsDuplicateField_ThrowsAlreadyJoinedMessage()
    {
        var rideEvent = new RideEvent
        {
            Id = 1,
            MaxParticipants = 10,
            Status = RideStatus.Planned,
            Participants = new List<RideParticipant>()
        };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(rideEvent);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ThrowsAsync(new DuplicateFieldException("Participant", "duplicate"));

        var act = async () => await _service.JoinRideAsync(1, userId: 7);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Već ste prijavljeni na ovaj događaj.");
    }

    [Fact]
    public async Task LeaveRideAsync_WhenRideDoesNotExist_ThrowsKeyNotFoundException()
    {
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(99, default))
            .ReturnsAsync((RideEvent?)null);

        var act = async () => await _service.LeaveRideAsync(99, userId: 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task LeaveRideAsync_WhenNotParticipant_ThrowsInvalidOperationException()
    {
        var rideEvent = new RideEvent { Id = 1, Participants = new List<RideParticipant>() };
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(rideEvent);

        var act = async () => await _service.LeaveRideAsync(1, userId: 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Niste prijavljeni na ovaj događaj.");
    }

    [Fact]
    public async Task LeaveRideAsync_WhenParticipant_RemovesParticipant()
    {
        var participant = new RideParticipant { UserId = 1, RideEventId = 1 };
        var rideEvent = new RideEvent { Id = 1, Participants = new List<RideParticipant> { participant } };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(rideEvent);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.LeaveRideAsync(1, userId: 1);

        _participantRepositoryMock.Verify(r => r.DeleteAsync(participant), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
