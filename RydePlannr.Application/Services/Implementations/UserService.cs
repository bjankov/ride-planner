using AutoMapper;
using RydePlannr.Application.DTOs.User;
using RydePlannr.Application.Services.Interfaces;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithRoleAsync(id, cancellationToken);
        return user is null ? null : _mapper.Map<UserResponseDto>(user);
    }

    public async Task<PublicUserResponseDto?> GetPublicByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithRoleAsync(id, cancellationToken);
        return user is null ? null : _mapper.Map<PublicUserResponseDto>(user);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllWithRoleAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<UserResponseDto>>(users);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id, cancellationToken)
                   ?? throw new KeyNotFoundException($"Korisnik s ID-em {id} nije pronađen.");

        await _unitOfWork.Repository<User>().DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}