using OptiExternalBlocks.Features.Templates.Models;

namespace OptiExternalBlocks.Features.Templates.Services.Abstractions;

/// <summary>
/// Service for managing external content templates.
/// </summary>
public interface ITemplateService
{
    Task<List<TemplateModel>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    Task<TemplateModel?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TemplateModel?> GetTemplateByContentTypeAsync(string contentTypeName, CancellationToken cancellationToken = default);
    Task<TemplateModel> CreateTemplateAsync(TemplateModel model, string username, CancellationToken cancellationToken = default);
    Task<TemplateModel> UpdateTemplateAsync(TemplateModel model, string username, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default);
}
