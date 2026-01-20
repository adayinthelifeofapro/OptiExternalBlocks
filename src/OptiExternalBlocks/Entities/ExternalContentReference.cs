using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptiExternalBlocks.Entities;

/// <summary>
/// Stores a reference to an external content item selected by an editor.
/// </summary>
[Table("tbl_OptiExternalBlocks_References")]
public class ExternalContentReference
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the external content item from the Graph API.
    /// </summary>
    [Required]
    [StringLength(512)]
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// The template ID used for rendering this content.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Navigation property to the template.
    /// </summary>
    [ForeignKey(nameof(TemplateId))]
    public ExternalContentTemplate? Template { get; set; }

    /// <summary>
    /// Cached title/name of the external content for display purposes.
    /// </summary>
    [StringLength(512)]
    public string? CachedTitle { get; set; }

    /// <summary>
    /// Cached thumbnail URL for display in the CMS.
    /// </summary>
    [StringLength(1024)]
    public string? CachedThumbnailUrl { get; set; }

    /// <summary>
    /// Cached JSON data from the last fetch (for offline/performance).
    /// </summary>
    public string? CachedData { get; set; }

    /// <summary>
    /// When the cached data was last updated.
    /// </summary>
    public DateTime? CacheUpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when this reference was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Username of who created this reference.
    /// </summary>
    [StringLength(256)]
    public string? CreatedBy { get; set; }
}
