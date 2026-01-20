using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptiExternalBlocks.Entities;

/// <summary>
/// Represents a Mustache template configuration for an external content type.
/// </summary>
[Table("tbl_OptiExternalBlocks_Templates")]
public class ExternalContentTemplate
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The external content type name from Graph API (e.g., "Article", "Product", "News").
    /// </summary>
    [Required]
    [StringLength(256)]
    public string ContentTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown to editors in the CMS.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of this external content type.
    /// </summary>
    [StringLength(1024)]
    public string? Description { get; set; }

    /// <summary>
    /// The Mustache template for rendering this content type in edit mode (preview).
    /// </summary>
    [Required]
    public string EditModeTemplate { get; set; } = string.Empty;

    /// <summary>
    /// The Mustache template for rendering this content type on the public site.
    /// </summary>
    [Required]
    public string RenderTemplate { get; set; } = string.Empty;

    /// <summary>
    /// The GraphQL query to fetch content of this type.
    /// </summary>
    [Required]
    public string GraphQLQuery { get; set; } = string.Empty;

    /// <summary>
    /// GraphQL query variables as JSON (optional).
    /// </summary>
    public string? GraphQLVariables { get; set; }

    /// <summary>
    /// Icon CSS class for the content picker (e.g., "epi-iconDocument").
    /// </summary>
    [StringLength(128)]
    public string IconClass { get; set; } = "epi-iconDocument";

    /// <summary>
    /// Field name in the external data to use for the title.
    /// If empty, falls back to common field names (name, title, displayName, etc.).
    /// </summary>
    [StringLength(128)]
    public string? TitleFieldName { get; set; }

    /// <summary>
    /// Field name in the external data to use for the thumbnail URL.
    /// If empty, falls back to common field names (thumbnail, image, thumbnailUrl, etc.).
    /// </summary>
    [StringLength(128)]
    public string? ThumbnailFieldName { get; set; }

    /// <summary>
    /// Whether this template is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order for display in the content picker.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Timestamp when this template was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Username of who created this template.
    /// </summary>
    [StringLength(256)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this template was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Username of who last modified this template.
    /// </summary>
    [StringLength(256)]
    public string? ModifiedBy { get; set; }
}
