using Microsoft.EntityFrameworkCore;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;

namespace RydePlannr.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdWithRoleAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllWithRoleAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.Role)
            .ToListAsync(cancellationToken);
    }
}