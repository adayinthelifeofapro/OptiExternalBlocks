using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OptiExternalBlocks.Entities;
using OptiExternalBlocks.Features.Templates.Models;
using OptiExternalBlocks.Features.Templates.Services.Abstractions;

namespace OptiExternalBlocks.Features.Templates.Services;

public class TemplateService : ITemplateService
{
    private readonly IOptiExternalBlocksDataContext _dataContext;
    private readonly IMemoryCache _cache;
    private const string CacheKeyAll = "templates:all";

    public TemplateService(IOptiExternalBlocksDataContext dataContext, IMemoryCache cache)
    {
        _dataContext = dataContext;
        _cache = cache;
    }

    public async Task<List<TemplateModel>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _dataContext.Templates
            .AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.DisplayName)
            .ToListAsync(cancellationToken);

        return templates.Select(MapToModel).ToList();
    }

    public async Task<TemplateModel?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _dataContext.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return template != null ? MapToModel(template) : null;
    }

    public async Task<TemplateModel?> GetTemplateByContentTypeAsync(string contentTypeName, CancellationToken cancellationToken = default)
    {
        var template = await _dataContext.Templates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ContentTypeName == contentTypeName && t.IsActive, cancellationToken);

        return template != null ? MapToModel(template) : null;
    }

    public async Task<TemplateModel> CreateTemplateAsync(TemplateModel model, string username, CancellationToken cancellationToken = default)
    {
        var entity = new ExternalContentTemplate
        {
            Id = Guid.NewGuid(),
            ContentTypeName = model.ContentTypeName,
            DisplayName = model.DisplayName,
            Description = model.Description,
            EditModeTemplate = model.EditModeTemplate,
            RenderTemplate = model.RenderTemplate,
            GraphQLQuery = model.GraphQLQuery,
            GraphQLVariables = model.GraphQLVariables,
            IconClass = model.IconClass,
            TitleFieldName = model.TitleFieldName,
            ThumbnailFieldName = model.ThumbnailFieldName,
            IsActive = model.IsActive,
            SortOrder = model.SortOrder,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        _dataContext.Templates.Add(entity);
        await ((DbContext)_dataContext).SaveChangesAsync(cancellationToken);

        InvalidateCache();

        return MapToModel(entity);
    }

    public async Task<TemplateModel> UpdateTemplateAsync(TemplateModel model, string username, CancellationToken cancellationToken = default)
    {
        var entity = await _dataContext.Templates
            .FirstOrDefaultAsync(t => t.Id == model.Id, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Template with ID {model.Id} not found.");
        }

        entity.ContentTypeName = model.ContentTypeName;
        entity.DisplayName = model.DisplayName;
        entity.Description = model.Description;
        entity.EditModeTemplate = model.EditModeTemplate;
        entity.RenderTemplate = model.RenderTemplate;
        entity.GraphQLQuery = model.GraphQLQuery;
        entity.GraphQLVariables = model.GraphQLVariables;
        entity.IconClass = model.IconClass;
        entity.TitleFieldName = model.TitleFieldName;
        entity.ThumbnailFieldName = model.ThumbnailFieldName;
        entity.IsActive = model.IsActive;
        entity.SortOrder = model.SortOrder;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = username;

        await ((DbContext)_dataContext).SaveChangesAsync(cancellationToken);

        InvalidateCache();

        return MapToModel(entity);
    }

    public async Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dataContext.Templates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity != null)
        {
            _dataContext.Templates.Remove(entity);
            await ((DbContext)_dataContext).SaveChangesAsync(cancellationToken);
            InvalidateCache();
        }
    }

    private void InvalidateCache()
    {
        _cache.Remove(CacheKeyAll);
    }

    private static TemplateModel MapToModel(ExternalContentTemplate entity)
    {
        return new TemplateModel
        {
            Id = entity.Id,
            ContentTypeName = entity.ContentTypeName,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            EditModeTemplate = entity.EditModeTemplate,
            RenderTemplate = entity.RenderTemplate,
            GraphQLQuery = entity.GraphQLQuery,
            GraphQLVariables = entity.GraphQLVariables,
            IconClass = entity.IconClass,
            TitleFieldName = entity.TitleFieldName,
            ThumbnailFieldName = entity.ThumbnailFieldName,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            ModifiedAt = entity.ModifiedAt,
            ModifiedBy = entity.ModifiedBy
        };
    }
}
