using RydePlannr.Domain.Entities;

namespace RydePlannr.Domain.Interfaces;

public interface IClubMemberRepository : IGenericRepository<ClubMember>
{
    Task<IReadOnlyList<ClubMember>> GetByClubAsync(int clubId, CancellationToken cancellationToken = default);

    Task<ClubMember?> GetMembershipAsync(int userId, int clubId, CancellationToken cancellationToken = default);

    Task<ClubMember?> GetActiveMembershipForUserAsync(int userId, CancellationToken cancellationToken = default);
}