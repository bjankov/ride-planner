namespace RydePlannr.Domain.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

    IRideRepository Rides { get; }
    IUserRepository Users { get; }
    IClubMemberRepository ClubMembers { get; }
    IClubRepository Clubs { get; }
    IRouteRepository Routes { get; }
    IRideMessageRepository Messages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}