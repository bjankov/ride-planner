using RydePlannr.Domain.Entities;

namespace RydePlannr.Domain.Interfaces;

public interface IClubRepository : IGenericRepository<Club>
{
    Task<Club?> GetByIdWithMembersAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Club>> GetAllWithMembersAsync(CancellationToken cancellationToken = default);
}
