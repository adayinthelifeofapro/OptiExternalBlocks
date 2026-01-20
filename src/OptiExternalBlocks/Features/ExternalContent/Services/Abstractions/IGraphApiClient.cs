using OptiExternalBlocks.Features.ExternalContent.Models;

namespace OptiExternalBlocks.Features.ExternalContent.Services.Abstractions;

/// <summary>
/// Client interface for communicating with the Graph API.
/// </summary>
public interface IGraphApiClient
{
    /// <summary>
    /// Execute a GraphQL query and return results.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <param name="variables">Optional variables as JSON.</param>
    /// <param name="endpointId">Optional specific endpoint ID to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query result as a dictionary.</returns>
    Task<Dictionary<string, object>?> ExecuteQueryAsync(
        string query,
        string? variables = null,
        Guid? endpointId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for external content items.
    /// </summary>
    /// <param name="request">The search request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search response with content items.</returns>
    Task<ExternalContentSearchResponse> SearchContentAsync(
        ExternalContentSearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific content item by its ID.
    /// </summary>
    /// <param name="templateId">The template ID for this content type.</param>
    /// <param name="externalId">The external content ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The content item or null if not found.</returns>
    Task<ExternalContentItem?> GetContentByIdAsync(
        Guid templateId,
        string externalId,
        CancellationToken cancellationToken = default);
}
