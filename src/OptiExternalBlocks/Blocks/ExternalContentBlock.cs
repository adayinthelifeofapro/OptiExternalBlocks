using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using OptiExternalBlocks.Common;

namespace OptiExternalBlocks.Blocks;

/// <summary>
/// A block that references external content from the Graph API.
/// Editors select content through the External Content picker, and the block
/// renders using the configured Mustache template.
/// </summary>
[ContentType(
    DisplayName = "External Content Block",
    GUID = "3b8e4f2a-9c1d-4e5f-8a7b-6c2d1e0f3a4b",
    Description = "Displays external content from the Graph API",
    GroupName = OptiExternalBlocksConstants.BlockCategory)]
public class ExternalContentBlock : BlockData
{
    /// <summary>
    /// The external content reference ID (links to ExternalContentReference entity).
    /// </summary>
    [Display(
        Name = "External Content",
        Description = "Select external content to display",
        GroupName = SystemTabNames.Content,
        Order = 10)]
    [UIHint("ExternalContentPicker")]
    public virtual string? ExternalContentId { get; set; }

    /// <summary>
    /// The template ID to use for rendering (stored for quick lookup).
    /// </summary>
    [Display(
        Name = "Template",
        Description = "The template used for this content type",
        GroupName = SystemTabNames.Content,
        Order = 20)]
    [ScaffoldColumn(false)]
    public virtual Guid TemplateId { get; set; }

    /// <summary>
    /// Cached title for display in edit mode.
    /// </summary>
    [Display(
        Name = "Content Title",
        Description = "Cached title of the selected content",
        GroupName = SystemTabNames.Content,
        Order = 30)]
    [ScaffoldColumn(false)]
    public virtual string? CachedTitle { get; set; }

    /// <summary>
    /// Optional CSS class to apply to the wrapper element.
    /// </summary>
    [Display(
        Name = "CSS Class",
        Description = "Optional CSS class for styling",
        GroupName = SystemTabNames.Content,
        Order = 40)]
    public virtual string? CssClass { get; set; }
}
