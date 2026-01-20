namespace OptiExternalBlocks.Features.ExternalContent.Models;

/// <summary>
/// Request model for searching external content.
/// </summary>
public class ExternalContentSearchRequest
{
    /// <summary>
    /// The template ID to use for this search.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Optional search query text.
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Optional language/locale filter.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Additional filter parameters as JSON.
    /// </summary>
    public string? AdditionalFilters { get; set; }
}
