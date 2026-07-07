using AutoMapper;
using FluentAssertions;
using Moq;
using RydePlannr.Application.DTOs.Location;
using RydePlannr.Application.Services.Implementations;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Tests.Services;

public class LocationServiceTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IGenericRepository<Location>> _repositoryMock;
    private readonly LocationService _service;

    public LocationServiceTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IGenericRepository<Location>>();

        _unitOfWorkMock.Setup(u => u.Repository<Location>()).Returns(_repositoryMock.Object);

        _service = new LocationService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedLocations()
    {
        var locations = new List<Location> { new() { Id = 1, Name = "Park" } };
        var expected = new List<LocationResponseDto> { new() { Id = 1, Name = "Park" } };

        _repositoryMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(locations);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<LocationResponseDto>>(locations)).Returns(expected);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        var location = new Location { Id = 1, Name = "Park" };
        var expected = new LocationResponseDto { Id = 1, Name = "Park" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(location);
        _mapperMock.Setup(m => m.Map<LocationResponseDto>(location)).Returns(expected);

        var result = await _service.GetByIdAsync(1);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Location?)null);

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsDto()
    {
        var dto = new CreateLocationDto { Name = "Park", Latitude = 45.8, Longitude = 16.0 };
        var mappedLocation = new Location { Name = dto.Name, Latitude = dto.Latitude, Longitude = dto.Longitude };
        var expected = new LocationResponseDto { Name = dto.Name };

        _mapperMock.Setup(m => m.Map<Location>(dto)).Returns(mappedLocation);
        _mapperMock.Setup(m => m.Map<LocationResponseDto>(mappedLocation)).Returns(expected);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.CreateAsync(dto);

        result.Should().Be(expected);
        _repositoryMock.Verify(r => r.AddAsync(mappedLocation, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotExists_ThrowsKeyNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Location?)null);

        var act = async () => await _service.DeleteAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesLocation()
    {
        var location = new Location { Id = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(location);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.DeleteAsync(1);

        _repositoryMock.Verify(r => r.DeleteAsync(location), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
