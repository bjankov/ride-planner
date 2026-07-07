using AutoMapper;
using FluentAssertions;
using Moq;
using RydePlannr.Application.DTOs.Route;
using RydePlannr.Application.Services.Implementations;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Tests.Services;

public class RouteServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRouteRepository> _routeRepositoryMock;
    private readonly Mock<IGenericRepository<Route>> _routeGenericRepositoryMock;
    private readonly Mock<IGenericRepository<Location>> _locationRepositoryMock;
    private readonly RouteService _service;

    public RouteServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _routeRepositoryMock = new Mock<IRouteRepository>();
        _routeGenericRepositoryMock = new Mock<IGenericRepository<Route>>();
        _locationRepositoryMock = new Mock<IGenericRepository<Location>>();

        _unitOfWorkMock.Setup(u => u.Routes).Returns(_routeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Route>()).Returns(_routeGenericRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Location>()).Returns(_locationRepositoryMock.Object);

        _service = new RouteService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    private static CreateRouteDto ValidCreateDto() => new()
    {
        Name = "Loop A",
        DistanceKm = 20,
        ElevationGainMeters = 100,
        StartLocationId = 1,
        EndLocationId = 2
    };

    [Fact]
    public async Task GetAllAsync_ReturnsMappedRoutes()
    {
        var routes = new List<Route> { new() { Id = 1, Name = "Loop A" } };
        var expected = new List<RouteResponseDto> { new() { Id = 1, Name = "Loop A" } };

        _routeRepositoryMock.Setup(r => r.GetAllWithLocationsAsync(default)).ReturnsAsync(routes);
        _mapperMock.Setup(m => m.Map<IReadOnlyList<RouteResponseDto>>(routes)).Returns(expected);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        var route = new Route { Id = 1, Name = "Loop A" };
        var expected = new RouteResponseDto { Id = 1, Name = "Loop A" };

        _routeRepositoryMock.Setup(r => r.GetByIdWithLocationsAsync(1, default)).ReturnsAsync(route);
        _mapperMock.Setup(m => m.Map<RouteResponseDto>(route)).Returns(expected);

        var result = await _service.GetByIdAsync(1);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        _routeRepositoryMock.Setup(r => r.GetByIdWithLocationsAsync(99, default)).ReturnsAsync((Route?)null);

        var result = await _service.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenStartLocationMissing_ThrowsKeyNotFoundException()
    {
        var dto = ValidCreateDto();
        _locationRepositoryMock.Setup(r => r.ExistsAsync(dto.StartLocationId, default)).ReturnsAsync(false);

        var act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenEndLocationMissing_ThrowsKeyNotFoundException()
    {
        var dto = ValidCreateDto();
        _locationRepositoryMock.Setup(r => r.ExistsAsync(dto.StartLocationId, default)).ReturnsAsync(true);
        _locationRepositoryMock.Setup(r => r.ExistsAsync(dto.EndLocationId, default)).ReturnsAsync(false);

        var act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsAndReturnsReloadedDto()
    {
        var dto = ValidCreateDto();
        var mappedRoute = new Route { Id = 1, Name = dto.Name };
        var reloadedRoute = new Route
        {
            Id = 1,
            Name = dto.Name,
            StartLocation = new Location { Name = "Start" },
            EndLocation = new Location { Name = "End" }
        };
        var expected = new RouteResponseDto { Name = dto.Name, StartLocationName = "Start", EndLocationName = "End" };

        _locationRepositoryMock.Setup(r => r.ExistsAsync(dto.StartLocationId, default)).ReturnsAsync(true);
        _locationRepositoryMock.Setup(r => r.ExistsAsync(dto.EndLocationId, default)).ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<Route>(dto)).Returns(mappedRoute);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _routeRepositoryMock.Setup(r => r.GetByIdWithLocationsAsync(mappedRoute.Id, default)).ReturnsAsync(reloadedRoute);
        _mapperMock.Setup(m => m.Map<RouteResponseDto>(reloadedRoute)).Returns(expected);

        var result = await _service.CreateAsync(dto);

        result.Should().Be(expected);
        _routeGenericRepositoryMock.Verify(r => r.AddAsync(mappedRoute, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotExists_ThrowsKeyNotFoundException()
    {
        _routeGenericRepositoryMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Route?)null);

        var act = async () => await _service.DeleteAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesRoute()
    {
        var route = new Route { Id = 1 };
        _routeGenericRepositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(route);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await _service.DeleteAsync(1);

        _routeGenericRepositoryMock.Verify(r => r.DeleteAsync(route), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
