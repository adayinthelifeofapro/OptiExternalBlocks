using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OptiExternalBlocks.Entities;

/// <summary>
/// Configuration for connecting to a Graph API endpoint.
/// </summary>
[Table("tbl_OptiExternalBlocks_Endpoints")]
public class GraphEndpointConfiguration
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Display name for this endpoint configuration.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The Graph API endpoint URL.
    /// </summary>
    [Required]
    [StringLength(1024)]
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// The single key for API authentication (if required).
    /// </summary>
    [StringLength(512)]
    public string? SingleKey { get; set; }

    /// <summary>
    /// The App Key for API authentication (if required).
    /// </summary>
    [StringLength(512)]
    public string? AppKey { get; set; }

    /// <summary>
    /// The App Secret for API authentication (if required).
    /// </summary>
    [StringLength(512)]
    public string? AppSecret { get; set; }

    /// <summary>
    /// Whether this is the default endpoint to use.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this endpoint configuration is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Username of who created this configuration.
    /// </summary>
    [StringLength(256)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when this configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Username of who last modified this configuration.
    /// </summary>
    [StringLength(256)]
    public string? ModifiedBy { get; set; }
}
