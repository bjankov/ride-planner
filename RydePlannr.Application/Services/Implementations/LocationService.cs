using AutoMapper;
using RydePlannr.Application.DTOs.Location;
using RydePlannr.Application.Services.Interfaces;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Application.Services.Implementations;

public class LocationService : ILocationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LocationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LocationResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var locations = await _unitOfWork.Repository<Location>().GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<LocationResponseDto>>(locations);
    }

    public async Task<LocationResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var location = await _unitOfWork.Repository<Location>().GetByIdAsync(id, cancellationToken);
        return location is null ? null : _mapper.Map<LocationResponseDto>(location);
    }

    public async Task<LocationResponseDto> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken = default)
    {
        var location = _mapper.Map<Location>(dto);
        await _unitOfWork.Repository<Location>().AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<LocationResponseDto>(location);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var location = await _unitOfWork.Repository<Location>().GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lokacija s ID-em {id} nije pronađena.");

        await _unitOfWork.Repository<Location>().DeleteAsync(location);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}