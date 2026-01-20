using OptiExternalBlocks.Features.Templates.Models;

namespace OptiExternalBlocks.Features.Templates.Services.Abstractions;

/// <summary>
/// Service for managing Graph endpoint configurations.
/// </summary>
public interface IEndpointService
{
    Task<List<EndpointModel>> GetAllEndpointsAsync(CancellationToken cancellationToken = default);
    Task<EndpointModel?> GetEndpointByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EndpointModel?> GetDefaultEndpointAsync(CancellationToken cancellationToken = default);
    Task<EndpointModel> CreateEndpointAsync(EndpointModel model, string username, CancellationToken cancellationToken = default);
    Task<EndpointModel> UpdateEndpointAsync(EndpointModel model, string username, CancellationToken cancellationToken = default);
    Task DeleteEndpointAsync(Guid id, CancellationToken cancellationToken = default);
    Task SetDefaultEndpointAsync(Guid id, CancellationToken cancellationToken = default);
}
