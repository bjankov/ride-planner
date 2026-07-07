using RydePlannr.Domain.Entities;

namespace RydePlannr.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithRoleAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllWithRoleAsync(CancellationToken cancellationToken = default);
}