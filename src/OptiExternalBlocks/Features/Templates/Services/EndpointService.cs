using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OptiExternalBlocks.Entities;
using OptiExternalBlocks.Features.Templates.Models;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;

namespace OptiExternalBlocks.Features.Templates.Services;

public class EndpointService : IEndpointService
{
    private readonly IOptiExternalBlocksDataContext _dataContext;
    private readonly IMemoryCache _cache;

    public EndpointService(IOptiExternalBlocksDataContext dataContext, IMemoryCache cache)
    {
        _dataContext = dataContext;
        _cache = cache;
    }

    public async Task<List<EndpointModel>> GetAllEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var endpoints = await _dataContext.Endpoints
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

        return endpoints.Select(MapToModel).ToList();
    }

    public async Task<EndpointModel?> GetEndpointByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var endpoint = await _dataContext.Endpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return endpoint != null ? MapToModel(endpoint) : null;
    }

    public async Task<EndpointModel?> GetDefaultEndpointAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = await _dataContext.Endpoints
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IsDefault && e.IsActive, cancellationToken);

        return endpoint != null ? MapToModel(endpoint) : null;
    }

    public async Task<EndpointModel> CreateEndpointAsync(EndpointModel model, string username, CancellationToken cancellationToken = default)
    {
        var entity = new GraphEndpointConfiguration
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            EndpointUrl = model.EndpointUrl,
            SingleKey = model.SingleKey,
            AppKey = model.AppKey,
            AppSecret = model.AppSecret,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        // If this is set as default, unset other defaults
        if (model.IsDefault)
        {
            await ClearDefaultsAsync(cancellationToken);
        }

        _dataContext.Endpoints.Add(entity);
        await ((DbContext)_dataContext).SaveChangesAsync(cancellationToken);

        return MapToModel(entity);
    }

    public async Task<EndpointModel> UpdateEndpointAsync(EndpointModel model, string username, CancellationToken cancellationToken = default)
    {
        var entity = await _dataContext.Endpoints
            .FirstOrDefaultAsync(e => e.Id == model.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Endpoint with ID {model.Id} not found.");
        }

        // If this is being set as default, unset other defaults
        if (model.IsDefault && !entity.IsDefault)
        {
            await ClearDefaultsAsync(cancellationToken);
        }

        entity.Name = model.Name;
        entity.EndpointUrl = model.EndpointUrl;
        entity.SingleKey = model.SingleKey;
        entity.AppKey = model.AppKey;
        entity.AppSecret = model.AppSecret;
        entity.IsDefault = model.IsDefault;
        entity.IsActive = model.IsActive;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = username;

        await ((DbContext)_dataContext).SaveChangesAsync(cancellationToken);

        return MapToModel(entity);
    }

    public async Task DeleteEndpointAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dataContext.Endpoints
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity != null)
        {
            _dataContext.Endpoints.Remove(entity);
            await ((DbContext)_dataContext).SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SetDefaultEndpointAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await ClearDefaultsAsync(cancellationToken);

        var entity = await _dataContext.Endpoints
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity != null)
        {
            entity.IsDefault = true;
            await ((DbContext)_dataContext).SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ClearDefaultsAsync(CancellationToken cancellationToken)
    {
        var defaults = await _dataContext.Endpoints
            .Where(e => e.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var endpoint in defaults)
        {
            endpoint.IsDefault = false;
        }
    }

    private static EndpointModel MapToModel(GraphEndpointConfiguration entity)
    {
        return new EndpointModel
        {
            Id = entity.Id,
            Name = entity.Name,
            EndpointUrl = entity.EndpointUrl,
            SingleKey = entity.SingleKey,
            AppKey = entity.AppKey,
            AppSecret = entity.AppSecret,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            ModifiedAt = entity.ModifiedAt,
            ModifiedBy = entity.ModifiedBy
        };
    }
}
