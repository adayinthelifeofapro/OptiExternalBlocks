using System.Text.Json;

namespace OptiExternalBlocks.Features.ExternalContent.Models;

/// <summary>
/// Represents an external content item retrieved from the Graph API.
/// </summary>
public class ExternalContentItem
{
    /// <summary>
    /// The unique identifier of this content item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The content type name.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Display title of the content item.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional thumbnail URL.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// The raw JSON data from the Graph API.
    /// </summary>
    public JsonDocument? RawData { get; set; }

    /// <summary>
    /// Dictionary representation of the data for Mustache rendering.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}
