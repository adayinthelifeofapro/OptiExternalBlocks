using System.ComponentModel.DataAnnotations;

namespace OptiExternalBlocks.Features.Templates.Models;

/// <summary>
/// View model for Graph endpoint configuration.
/// </summary>
public class EndpointModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(256, ErrorMessage = "Name cannot exceed 256 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Endpoint URL is required.")]
    [StringLength(1024, ErrorMessage = "Endpoint URL cannot exceed 1024 characters.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string EndpointUrl { get; set; } = string.Empty;

    [StringLength(512)]
    public string? SingleKey { get; set; }

    [StringLength(512)]
    public string? AppKey { get; set; }

    [StringLength(512)]
    public string? AppSecret { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
