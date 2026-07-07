using AutoMapper;
using RydePlannr.Application.DTOs.Route;
using RydePlannr.Application.Services.Interfaces;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Application.Services.Implementations;

public class RouteService : IRouteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RouteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RouteResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var routes = await _unitOfWork.Routes.GetAllWithLocationsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<RouteResponseDto>>(routes);
    }

    public async Task<RouteResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var route = await _unitOfWork.Routes.GetByIdWithLocationsAsync(id, cancellationToken);
        return route is null ? null : _mapper.Map<RouteResponseDto>(route);
    }

    public async Task<RouteResponseDto> CreateAsync(CreateRouteDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Repository<Location>().ExistsAsync(dto.StartLocationId, cancellationToken))
            throw new KeyNotFoundException($"Početna lokacija s ID-em {dto.StartLocationId} nije pronađena.");

        if (!await _unitOfWork.Repository<Location>().ExistsAsync(dto.EndLocationId, cancellationToken))
            throw new KeyNotFoundException($"Završna lokacija s ID-em {dto.EndLocationId} nije pronađena.");

        var route = _mapper.Map<Route>(dto);
        await _unitOfWork.Repository<Route>().AddAsync(route, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdRoute = await _unitOfWork.Routes.GetByIdWithLocationsAsync(route.Id, cancellationToken);
        return _mapper.Map<RouteResponseDto>(createdRoute);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var route = await _unitOfWork.Repository<Route>().GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Ruta s ID-em {id} nije pronađena.");

        await _unitOfWork.Repository<Route>().DeleteAsync(route);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}