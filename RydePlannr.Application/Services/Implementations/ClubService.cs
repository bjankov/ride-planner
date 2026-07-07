using AutoMapper;
using RydePlannr.Application.DTOs.Club;
using RydePlannr.Application.Services.Interfaces;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Application.Services.Implementations;

public class ClubService : IClubService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ClubService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ClubResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clubs = await _unitOfWork.Clubs.GetAllWithMembersAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ClubResponseDto>>(clubs);
    }

    public async Task<ClubResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var club = await _unitOfWork.Clubs.GetByIdWithMembersAsync(id, cancellationToken);
        return club is null ? null : _mapper.Map<ClubResponseDto>(club);
    }

    public async Task<ClubResponseDto> CreateAsync(
        CreateClubDto dto, CancellationToken cancellationToken = default)
    {
        var club = _mapper.Map<Club>(dto);
        club.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Club>().AddAsync(club, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ClubResponseDto>(club);
    }

    public async Task<IReadOnlyList<ClubMemberResponseDto>> GetMembersAsync(
        int clubId, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Repository<Club>().ExistsAsync(clubId, cancellationToken))
            throw new KeyNotFoundException($"Klub s ID-em {clubId} nije pronađen.");

        var clubMembers = await _unitOfWork.ClubMembers.GetByClubAsync(clubId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ClubMemberResponseDto>>(clubMembers);
    }

    public async Task JoinAsync(int clubId, int userId, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Repository<Club>().ExistsAsync(clubId, cancellationToken))
            throw new KeyNotFoundException($"Klub s ID-em {clubId} nije pronađen.");

        var existingMembership = await _unitOfWork.ClubMembers.GetActiveMembershipForUserAsync(userId, cancellationToken);

        if (existingMembership is not null)
        {
            throw existingMembership.ClubId == clubId
                ? new InvalidOperationException("Već ste član ovog kluba.")
                : new InvalidOperationException("Već ste član drugog kluba. Napustite ga prije pristupanja novom.");
        }

        var membership = new ClubMember
        {
            UserId = userId,
            ClubId = clubId,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ClubMember>().AddAsync(membership, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateFieldException)
        {
            throw new InvalidOperationException("Već ste član ovog kluba.");
        }
    }

    public async Task LeaveAsync(int clubId, int userId, CancellationToken cancellationToken = default)
    {
        var membership = await _unitOfWork.ClubMembers.GetMembershipAsync(userId, clubId, cancellationToken)
            ?? throw new InvalidOperationException("Niste član ovog kluba.");

        await _unitOfWork.Repository<ClubMember>().DeleteAsync(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var club = await _unitOfWork.Repository<Club>().GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Klub s ID-em {id} nije pronađen.");

        await _unitOfWork.Repository<Club>().DeleteAsync(club);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}