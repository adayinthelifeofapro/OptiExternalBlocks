namespace OptiExternalBlocks.Features.ExternalContent.Models;

/// <summary>
/// Response model for external content searches.
/// </summary>
public class ExternalContentSearchResponse
{
    /// <summary>
    /// The list of content items found.
    /// </summary>
    public List<ExternalContentItem> Items { get; set; } = new();

    /// <summary>
    /// Total number of items matching the search.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size used.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Whether there are more pages available.
    /// </summary>
    public bool HasMorePages => Page * PageSize < TotalCount;
}
