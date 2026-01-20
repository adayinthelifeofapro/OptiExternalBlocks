using System.ComponentModel.DataAnnotations;

namespace OptiExternalBlocks.Features.Templates.Models;

/// <summary>
/// View model for external content template configuration.
/// </summary>
public class TemplateModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Content type name is required.")]
    [StringLength(256, ErrorMessage = "Content type name cannot exceed 256 characters.")]
    public string ContentTypeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display name is required.")]
    [StringLength(256, ErrorMessage = "Display name cannot exceed 256 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(1024, ErrorMessage = "Description cannot exceed 1024 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Edit mode template is required.")]
    public string EditModeTemplate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Render template is required.")]
    public string RenderTemplate { get; set; } = string.Empty;

    [Required(ErrorMessage = "GraphQL query is required.")]
    public string GraphQLQuery { get; set; } = string.Empty;

    public string? GraphQLVariables { get; set; }

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

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
