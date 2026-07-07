using AutoMapper;
using FluentAssertions;
using Moq;
using RydePlannr.Application.DTOs.Message;
using RydePlannr.Application.Services.Implementations;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Tests.Services;

public class MessageServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IGenericRepository<RideEvent>> _rideEventRepositoryMock;
    private readonly Mock<IRideMessageRepository> _messageRepositoryMock;
    private readonly Mock<IGenericRepository<RideMessage>> _messageGenericRepositoryMock;
    private readonly MessageService _service;

    public MessageServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _rideEventRepositoryMock = new Mock<IGenericRepository<RideEvent>>();
        _messageRepositoryMock = new Mock<IRideMessageRepository>();
        _messageGenericRepositoryMock = new Mock<IGenericRepository<RideMessage>>();

        _unitOfWorkMock.Setup(u => u.Repository<RideEvent>()).Returns(_rideEventRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Messages).Returns(_messageRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<RideMessage>()).Returns(_messageGenericRepositoryMock.Object);

        _service = new MessageService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllByRideEventAsync_WhenRideDoesNotExist_ThrowsKeyNotFoundException()
    {
        _rideEventRepositoryMock.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        var act = async () => await _service.GetAllByRideEventAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllByRideEventAsync_WhenRideExists_ReturnsMappedMessages()
    {
        var messages = new List<RideMessage> { new() { Id = 1, Content = "Hi" } };
        var expected = new List<MessageResponseDto> { new() { Id = 1, Content = "Hi" } };

        _rideEventRepositoryMock.Setup(r => r.ExistsAsync(1, default)).ReturnsAsync(true);
        _messageRepositoryMock.Setup(r => r.GetByRideEventWithUserAsync(1, default)).ReturnsAsync(messages);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<MessageResponseDto>>(messages)).Returns(expected);

        var result = await _service.GetAllByRideEventAsync(1);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task CreateAsync_WhenRideDoesNotExist_ThrowsKeyNotFoundException()
    {
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(99, default))
            .ReturnsAsync((RideEvent?)null);

        var act = async () => await _service.CreateAsync(99, userId: 1, new CreateMessageDto { Content = "Hi" });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenUserNotParticipant_ThrowsForbiddenException()
    {
        var rideEvent = new RideEvent
        {
            Id = 1,
            Participants = new List<RideParticipant> { new() { UserId = 5, User = new User() } }
        };
        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(rideEvent);

        var act = async () => await _service.CreateAsync(1, userId: 99, new CreateMessageDto { Content = "Hi" });

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Samo sudionici događaja mogu slati poruke.");
    }

    [Fact]
    public async Task CreateAsync_WhenUserIsParticipant_PersistsMessageAndReturnsDto()
    {
        var participantUser = new User { Id = 5, Username = "rider" };
        var rideEvent = new RideEvent
        {
            Id = 1,
            Participants = new List<RideParticipant> { new() { UserId = 5, User = participantUser } }
        };
        var dto = new CreateMessageDto { Content = "See you there!" };
        var expected = new MessageResponseDto { Content = dto.Content, Username = "rider" };

        _unitOfWorkMock.Setup(u => u.Rides.GetRideWithParticipantsAsync(1, default)).ReturnsAsync(rideEvent);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<MessageResponseDto>(It.IsAny<RideMessage>())).Returns(expected);

        var result = await _service.CreateAsync(1, userId: 5, dto);

        result.Should().Be(expected);
        _messageGenericRepositoryMock.Verify(r => r.AddAsync(
            It.Is<RideMessage>(m => m.Content == dto.Content && m.UserId == 5 && m.RideEventId == 1 && m.User == participantUser),
            default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
