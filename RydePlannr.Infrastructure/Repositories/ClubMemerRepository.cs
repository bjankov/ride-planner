using Microsoft.EntityFrameworkCore;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;

namespace RydePlannr.Infrastructure.Repositories;

public class ClubMemberRepository : GenericRepository<ClubMember>, IClubMemberRepository
{
    public ClubMemberRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ClubMember>> GetByClubAsync(int clubId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.User)
            .Where(m => m.ClubId == clubId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClubMember?> GetMembershipAsync(int userId, int clubId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(m => m.UserId == userId && m.ClubId == clubId, cancellationToken);
    }

    public async Task<ClubMember?> GetActiveMembershipForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
    }
}