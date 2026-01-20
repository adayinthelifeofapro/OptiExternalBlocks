using OptiExternalBlocks.Features.ExternalContent.Models;

namespace OptiExternalBlocks.Features.Templates.Services.Abstractions;

/// <summary>
/// Service for rendering external content using Mustache templates.
/// </summary>
public interface ITemplateRenderingService
{
    /// <summary>
    /// Render content using the edit mode template.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="content">The content item to render.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rendered HTML for edit mode.</returns>
    Task<string> RenderEditModeAsync(
        Guid templateId,
        ExternalContentItem content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Render content using the public site template.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="content">The content item to render.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rendered HTML for public display.</returns>
    Task<string> RenderPublicAsync(
        Guid templateId,
        ExternalContentItem content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate a Mustache template string.
    /// </summary>
    /// <param name="template">The template to validate.</param>
    /// <returns>Validation result with any errors.</returns>
    TemplateValidationResult ValidateTemplate(string template);

    /// <summary>
    /// Preview a template with sample data.
    /// </summary>
    /// <param name="template">The Mustache template.</param>
    /// <param name="sampleDataJson">Sample data as JSON.</param>
    /// <returns>Rendered preview HTML.</returns>
    string PreviewTemplate(string template, string sampleDataJson);
}

/// <summary>
/// Result of template validation.
/// </summary>
public class TemplateValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
