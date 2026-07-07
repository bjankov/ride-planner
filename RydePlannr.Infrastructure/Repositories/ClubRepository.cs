using Microsoft.EntityFrameworkCore;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;

namespace RydePlannr.Infrastructure.Repositories;

public class ClubRepository : GenericRepository<Club>, IClubRepository
{
    public ClubRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Club?> GetByIdWithMembersAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Club>> GetAllWithMembersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Members)
            .ToListAsync(cancellationToken);
    }
}
