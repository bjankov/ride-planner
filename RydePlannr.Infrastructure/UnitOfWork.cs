using Microsoft.EntityFrameworkCore;
using Npgsql;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;
using RydePlannr.Infrastructure.Repositories;

namespace RydePlannr.Infrastructure;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();

    private readonly Lazy<IRideRepository> _rides = new(() => new RideRepository(context));
    private readonly Lazy<IUserRepository> _users = new(() => new UserRepository(context));
    private readonly Lazy<IClubMemberRepository> _clubMembers = new(() => new ClubMemberRepository(context));
    private readonly Lazy<IClubRepository> _clubs = new(() => new ClubRepository(context));
    private readonly Lazy<IRouteRepository> _routes = new(() => new RouteRepository(context));
    private readonly Lazy<IRideMessageRepository> _messages = new(() => new RideMessageRepository(context));

    public IRideRepository Rides => _rides.Value;
    public IUserRepository Users => _users.Value;
    public IClubMemberRepository ClubMembers => _clubMembers.Value;
    public IClubRepository Clubs => _clubs.Value;
    public IRouteRepository Routes => _routes.Value;
    public IRideMessageRepository Messages => _messages.Value;

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (_repositories.TryGetValue(type, out var existing))
            return (IGenericRepository<TEntity>)existing;

        var repository = new GenericRepository<TEntity>(context);
        _repositories[type] = repository;
        return repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pgEx)
        {
            var fieldName = MapConstraintToFieldName(pgEx.ConstraintName);
            throw new DuplicateFieldException(
                fieldName,
                $"A record with this {fieldName.ToLowerInvariant()} already exists.");
        }
    }
    
    private static string MapConstraintToFieldName(string? constraintName) => constraintName switch
    {
        "IX_Users_Email" => "Email",
        "IX_Users_Username" => "Username",
        "IX_RideParticipants_RideEventId_UserId" => "Participant",
        "IX_ClubMembers_UserId_ClubId" => "Membership",
        _ => constraintName ?? "Unknown"
    };
}