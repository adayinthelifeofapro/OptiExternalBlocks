using Microsoft.EntityFrameworkCore;

namespace OptiExternalBlocks.Entities;

public interface IOptiExternalBlocksDataContext
{
    DbSet<ExternalContentTemplate> Templates { get; set; }
    DbSet<ExternalContentReference> References { get; set; }
    DbSet<GraphEndpointConfiguration> Endpoints { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
