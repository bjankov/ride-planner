using RydePlannr.Domain.Entities;

namespace RydePlannr.Domain.Interfaces;

public interface IRouteRepository : IGenericRepository<Route>
{
    Task<Route?> GetByIdWithLocationsAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Route>> GetAllWithLocationsAsync(CancellationToken cancellationToken = default);
}
